using SecurityAwareness.Common.Enums;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface ICampaignScheduleService
{
    Task<IReadOnlyList<CampaignSchedule>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CampaignSchedule>> GetByCampaignAsync(int campaignId, CancellationToken ct = default);
    Task<CampaignSchedule?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CampaignSchedule> CreateAsync(CampaignSchedule schedule, CancellationToken ct = default);
    Task UpdateAsync(CampaignSchedule schedule, CancellationToken ct = default);
    Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
