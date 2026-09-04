using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class CampaignRepository : ICampaignRepository
{
    private readonly AwarenessDbContext _db;
    public CampaignRepository(AwarenessDbContext db) => _db = db;

    public Task<PhishingCampaign?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.PhishingCampaigns.FirstOrDefaultAsync(c => c.CampaignId == id, ct);

    public async Task<IReadOnlyList<PhishingCampaign>> GetAllAsync(CancellationToken ct = default)
        => await _db.PhishingCampaigns.AsNoTracking().OrderByDescending(c => c.StartAt).ToListAsync(ct);

    public async Task AddAsync(PhishingCampaign campaign, CancellationToken ct = default)
        => await _db.PhishingCampaigns.AddAsync(campaign, ct);

    public Task UpdateAsync(PhishingCampaign campaign, CancellationToken ct = default)
    {
        _db.PhishingCampaigns.Update(campaign);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        => _db.PhishingCampaigns.AnyAsync(c => c.Code == code, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
