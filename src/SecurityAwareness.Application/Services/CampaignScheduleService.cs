using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class CampaignScheduleService : ICampaignScheduleService
{
    private readonly ICampaignScheduleRepository _repo;
    private readonly IAuditService _audit;
    private readonly ILogger<CampaignScheduleService> _logger;

    public CampaignScheduleService(
        ICampaignScheduleRepository repo,
        IAuditService audit,
        ILogger<CampaignScheduleService> logger)
    {
        _repo = repo;
        _audit = audit;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CampaignSchedule>> GetAllAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task<IReadOnlyList<CampaignSchedule>> GetByCampaignAsync(int campaignId, CancellationToken ct = default)
        => await _repo.GetByCampaignAsync(campaignId, ct);

    public async Task<CampaignSchedule?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct);

    public async Task<CampaignSchedule> CreateAsync(CampaignSchedule schedule, CancellationToken ct = default)
    {
        if (schedule.CampaignId == 0) throw new ArgumentException("CampaignId is required.");
        if (schedule.EndAt.HasValue && schedule.EndAt <= schedule.StartAt)
            throw new ArgumentException("EndAt must be after StartAt.");

        schedule.CreatedAt = DateTime.Now;
        schedule.IsActive = true;
        schedule.NextRunAt = schedule.StartAt;
        await _repo.AddAsync(schedule, ct);
        await _repo.SaveChangesAsync(ct);

        await _audit.WriteAsync("Create", "CampaignSchedule", schedule.ScheduleId.ToString(),
            "admin", $"campaign={schedule.CampaignId}, type={schedule.ScheduleType}", ct);

        _logger.LogInformation("Created CampaignSchedule {Id} for Campaign {Cid}",
            schedule.ScheduleId, schedule.CampaignId);
        return schedule;
    }

    public async Task UpdateAsync(CampaignSchedule schedule, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(schedule, ct);
        await _repo.SaveChangesAsync(ct);
        await _audit.WriteAsync("Update", "CampaignSchedule", schedule.ScheduleId.ToString(),
            "admin", null, ct);
    }

    public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var s = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Schedule {id} not found.");
        s.IsActive = isActive;
        await _repo.UpdateAsync(s, ct);
        await _repo.SaveChangesAsync(ct);
        await _audit.WriteAsync(isActive ? "Enable" : "Disable", "CampaignSchedule", id.ToString(),
            "admin", null, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var s = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Schedule {id} not found.");
        await _repo.DeleteAsync(s, ct);
        await _repo.SaveChangesAsync(ct);
        await _audit.WriteAsync("Delete", "CampaignSchedule", id.ToString(), "admin", null, ct);
    }
}
