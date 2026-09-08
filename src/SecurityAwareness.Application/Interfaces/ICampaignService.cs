using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface ICampaignService
{
    Task<IReadOnlyList<PhishingCampaign>> GetAllAsync(CancellationToken ct = default);
    Task<PhishingCampaign?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PhishingCampaign> CreateAsync(PhishingCampaign campaign, string actor, CancellationToken ct = default);
    Task UpdateAsync(PhishingCampaign campaign, CancellationToken ct = default);
    Task StartAsync(int campaignId, CancellationToken ct = default);
    Task EndAsync(int campaignId, CancellationToken ct = default);

    /// <summary>Returns active departments for the campaign-creation department picker.</summary>
    Task<IReadOnlyList<Department>> GetDepartmentsAsync(CancellationToken ct = default);

    /// <summary>
    /// Generate TrackingTokens + TokenUrls for all active employees and add Assignments.
    /// </summary>
    Task<int> CreateAssignmentsAsync(int campaignId, string baseUrl, CancellationToken ct = default);

    /// <summary>
    /// Queue dispatch of all Pending assignments using selected mailboxes (round-robin).
    /// Returns total queued count.
    /// </summary>
    Task<int> DispatchAsync(int campaignId, IReadOnlyList<int> mailboxIds, CancellationToken ct = default);
}
