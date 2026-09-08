using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class CampaignScheduleRepository : ICampaignScheduleRepository
{
    private readonly AwarenessDbContext _db;
    public CampaignScheduleRepository(AwarenessDbContext db) => _db = db;

    public Task<CampaignSchedule?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.CampaignSchedules.FirstOrDefaultAsync(s => s.ScheduleId == id, ct);

    public async Task<IReadOnlyList<CampaignSchedule>> GetAllAsync(CancellationToken ct = default)
        => await _db.CampaignSchedules.AsNoTracking().OrderBy(s => s.NextRunAt).ToListAsync(ct);

    public async Task<IReadOnlyList<CampaignSchedule>> GetByCampaignAsync(int campaignId, CancellationToken ct = default)
        => await _db.CampaignSchedules.AsNoTracking()
            .Where(s => s.CampaignId == campaignId)
            .OrderBy(s => s.NextRunAt).ToListAsync(ct);

    public async Task AddAsync(CampaignSchedule schedule, CancellationToken ct = default)
        => await _db.CampaignSchedules.AddAsync(schedule, ct);

    public Task UpdateAsync(CampaignSchedule schedule, CancellationToken ct = default)
    {
        _db.CampaignSchedules.Update(schedule);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(CampaignSchedule schedule, CancellationToken ct = default)
    {
        _db.CampaignSchedules.Remove(schedule);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
