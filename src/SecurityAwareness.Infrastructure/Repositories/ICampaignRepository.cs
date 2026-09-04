using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface ICampaignRepository
{
    Task<PhishingCampaign?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PhishingCampaign>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(PhishingCampaign campaign, CancellationToken ct = default);
    Task UpdateAsync(PhishingCampaign campaign, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
