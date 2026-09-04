using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class CampaignService : ICampaignService
{
    private readonly ICampaignRepository _campaigns;
    private readonly IAssignmentRepository _assignments;
    private readonly IEmployeeRepository _employees;
    private readonly ISenderMailboxRepository _mailboxes;
    private readonly IEmailQueueChannel _queue;
    private readonly EmailMessageRenderer _renderer;
    private readonly IAuditService _audit;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(
        ICampaignRepository campaigns,
        IAssignmentRepository assignments,
        IEmployeeRepository employees,
        ISenderMailboxRepository mailboxes,
        IEmailQueueChannel queue,
        EmailMessageRenderer renderer,
        IAuditService audit,
        ILogger<CampaignService> logger)
    {
        _campaigns = campaigns;
        _assignments = assignments;
        _employees = employees;
        _mailboxes = mailboxes;
        _queue = queue;
        _renderer = renderer;
        _audit = audit;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PhishingCampaign>> GetAllAsync(CancellationToken ct = default)
        => await _campaigns.GetAllAsync(ct);

    public async Task<PhishingCampaign?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _campaigns.GetByIdAsync(id, ct);

    public async Task<PhishingCampaign> CreateAsync(PhishingCampaign campaign, string actor, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(campaign.Code))
            throw new ArgumentException("Code is required.", nameof(campaign));
        if (string.IsNullOrWhiteSpace(campaign.Title))
            throw new ArgumentException("Title is required.", nameof(campaign));
        if (campaign.EndAt <= campaign.StartAt)
            throw new ArgumentException("EndAt must be after StartAt.", nameof(campaign));

        if (await _campaigns.ExistsByCodeAsync(campaign.Code, ct))
            throw new InvalidOperationException($"Code '{campaign.Code}' already exists.");

        campaign.Status = (byte)0; // Draft
        campaign.CreatedBy = actor;
        campaign.CreatedAt = DateTime.Now;
        await _campaigns.AddAsync(campaign, ct);
        await _campaigns.SaveChangesAsync(ct);

        await _audit.WriteAsync("Create", "Campaign", campaign.CampaignId.ToString(), actor, null, ct);
        _logger.LogInformation("Created Campaign {Id} - {Code} by {Actor}", campaign.CampaignId, campaign.Code, actor);
        return campaign;
    }

    public async Task UpdateAsync(PhishingCampaign campaign, CancellationToken ct = default)
    {
        campaign.UpdatedAt = DateTime.Now;
        await _campaigns.UpdateAsync(campaign, ct);
        await _campaigns.SaveChangesAsync(ct);
        await _audit.WriteAsync("Update", "Campaign", campaign.CampaignId.ToString(), "system", null, ct);
    }

    public async Task StartAsync(int campaignId, CancellationToken ct = default)
    {
        var c = await _campaigns.GetByIdAsync(campaignId, ct)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");
        if (c.Status != (byte)0)
            throw new InvalidOperationException($"Only Draft campaigns can be started (current status={c.Status}).");

        c.Status = (byte)1; // Active
        c.UpdatedAt = DateTime.Now;
        await _campaigns.UpdateAsync(c, ct);
        await _campaigns.SaveChangesAsync(ct);

        await _audit.WriteAsync("Start", "Campaign", campaignId.ToString(), "system", null, ct);
        _logger.LogInformation("Started Campaign {Id}", campaignId);
    }

    public async Task EndAsync(int campaignId, CancellationToken ct = default)
    {
        var c = await _campaigns.GetByIdAsync(campaignId, ct)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");
        if (c.Status != (byte)1)
            throw new InvalidOperationException($"Only Active campaigns can be ended (current status={c.Status}).");

        c.Status = (byte)2; // Completed
        c.UpdatedAt = DateTime.Now;
        await _campaigns.UpdateAsync(c, ct);
        await _campaigns.SaveChangesAsync(ct);

        await _audit.WriteAsync("End", "Campaign", campaignId.ToString(), "system", null, ct);
        _logger.LogInformation("Ended Campaign {Id}", campaignId);
    }

    public async Task<int> CreateAssignmentsAsync(int campaignId, string baseUrl, CancellationToken ct = default)
    {
        var c = await _campaigns.GetByIdAsync(campaignId, ct)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");

        var employees = await _employees.GetActiveAsync(ct);
        var existing = await _assignments.GetByCampaignAsync(campaignId, ct);
        var existingSet = existing.Select(a => a.EmployeeId).ToHashSet();

        var toAdd = new List<EmployeeCampaignAssignment>();
        foreach (var e in employees.Where(e => !existingSet.Contains(e.EmployeeId)))
        {
            var token = Guid.NewGuid().ToString();
            toAdd.Add(new EmployeeCampaignAssignment
            {
                CampaignId = campaignId,
                EmployeeId = e.EmployeeId,
                TrackingToken = token,
                Status = (byte)0, // Pending
                TokenUrl = $"{baseUrl.TrimEnd('/')}/r/{token}"
            });
        }

        if (toAdd.Count > 0)
        {
            await _assignments.AddRangeAsync(toAdd, ct);
            await _assignments.SaveChangesAsync(ct);
        }
        _logger.LogInformation("Created {Count} assignments for Campaign {Id}", toAdd.Count, campaignId);
        return toAdd.Count;
    }

    public async Task<int> DispatchAsync(int campaignId, IReadOnlyList<int> mailboxIds, CancellationToken ct = default)
    {
        if (mailboxIds.Count == 0) throw new ArgumentException("At least one mailbox required.");

        var campaign = await _campaigns.GetByIdAsync(campaignId, ct)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");
        if (campaign.Status != (byte)Common.Enums.CampaignStatus.Active)
            throw new InvalidOperationException("Only Active campaigns can be dispatched.");

        // Load all active mailboxes
        var allMailboxes = await _mailboxes.GetActiveAsync(ct);
        var selectedMailboxes = allMailboxes.Where(m => mailboxIds.Contains(m.SenderMailboxId)).ToList();
        if (selectedMailboxes.Count == 0)
            throw new InvalidOperationException("None of the selected mailboxes are active.");

        // Load pending assignments
        var assignments = (await _assignments.GetByCampaignAsync(campaignId, ct))
            .Where(a => a.Status == (byte)Common.Enums.AssignmentStatus.Pending && a.Employee is not null)
            .ToList();
        if (assignments.Count == 0) return 0;

        var baseUrl = campaign.Assignments.FirstOrDefault()?.TokenUrl?.Substring(0,
            campaign.Assignments.FirstOrDefault()!.TokenUrl!.IndexOf("/r/", StringComparison.Ordinal)) ?? "http://localhost:5098";

        int queued = 0;
        for (int i = 0; i < assignments.Count; i++)
        {
            var assignment = assignments[i];
            var mailbox = selectedMailboxes[i % selectedMailboxes.Count];
            var employee = assignment.Employee!;

            var email = _renderer.Render(campaign, assignment, employee, mailbox, baseUrl);
            await _queue.Writer.WriteAsync(email, ct);

            // Mark Dispatched and timestamp
            assignment.Status = (byte)Common.Enums.AssignmentStatus.Dispatched;
            assignment.DispatchedAt = DateTime.Now;
            await _assignments.UpdateAsync(assignment, ct);

            queued++;
        }
        await _assignments.SaveChangesAsync(ct);
        await _audit.WriteAsync("Dispatch", "Campaign", campaignId.ToString(), "system",
            $"queued={queued}, mailboxes=[{string.Join(",", selectedMailboxes.Select(m => m.Email))}]", ct);
        _logger.LogInformation("Dispatched Campaign {Id}: queued {Count}", campaignId, queued);
        return queued;
    }
}
