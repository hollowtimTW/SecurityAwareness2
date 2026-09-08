using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class EmailDeliveryLogRepository : IEmailDeliveryLogRepository
{
    private readonly AwarenessDbContext _db;
    public EmailDeliveryLogRepository(AwarenessDbContext db) => _db = db;

    public async Task<EmailDeliveryPage> QueryAsync(EmailDeliveryQuery q, CancellationToken ct = default)
    {
        var page = q.Page <= 0 ? 1 : q.Page;
        var size = q.PageSize <= 0 ? 50 : Math.Min(q.PageSize, 200);

        IQueryable<EmailDeliveryLog> query = _db.EmailDeliveryLogs
            .AsNoTracking()
            .Include(l => l.Assignment).ThenInclude(a => a!.Employee)
            .Include(l => l.SenderMailbox);

        if (q.CampaignId.HasValue)
            query = query.Where(l => l.AssignmentId > 0 &&
                _db.EmployeeCampaignAssignments.Any(a => a.AssignmentId == l.AssignmentId && a.CampaignId == q.CampaignId));
        if (q.Status.HasValue)
            query = query.Where(l => l.Status == q.Status.Value);
        if (q.MailboxId.HasValue)
            query = query.Where(l => l.SenderMailboxId == q.MailboxId);
        if (q.FromUtc.HasValue)
            query = query.Where(l => l.SentAt >= q.FromUtc.Value);
        if (q.ToUtc.HasValue)
            query = query.Where(l => l.SentAt <= q.ToUtc.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);
        return new EmailDeliveryPage(items, total, page, size);
    }

    public async Task<EmailDeliveryLog?> GetByIdAsync(long id, CancellationToken ct = default)
        => await _db.EmailDeliveryLogs.AsNoTracking()
            .Include(l => l.Assignment).ThenInclude(a => a!.Employee)
            .Include(l => l.Assignment).ThenInclude(a => a!.Campaign)
            .Include(l => l.SenderMailbox)
            .FirstOrDefaultAsync(l => l.LogId == id, ct);

    public async Task<IReadOnlyList<int>> GetCampaignIdsByMailboxAsync(int mailboxId, CancellationToken ct = default)
        => await _db.EmailDeliveryLogs.AsNoTracking()
            .Where(l => l.SenderMailboxId == mailboxId && l.AssignmentId > 0)
            .Select(l => _db.EmployeeCampaignAssignments
                .Where(a => a.AssignmentId == l.AssignmentId)
                .Select(a => a.CampaignId).FirstOrDefault())
            .Distinct()
            .ToListAsync(ct);

    public async Task AddAsync(EmailDeliveryLog log, CancellationToken ct = default)
        => await _db.EmailDeliveryLogs.AddAsync(log, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
