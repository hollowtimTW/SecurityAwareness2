using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface ICampaignScheduleRepository
{
    Task<CampaignSchedule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CampaignSchedule>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CampaignSchedule>> GetByCampaignAsync(int campaignId, CancellationToken ct = default);
    Task AddAsync(CampaignSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(CampaignSchedule schedule, CancellationToken ct = default);
    Task DeleteAsync(CampaignSchedule schedule, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
