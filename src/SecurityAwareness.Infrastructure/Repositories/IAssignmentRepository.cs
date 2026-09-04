using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface IAssignmentRepository
{
    Task<EmployeeCampaignAssignment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EmployeeCampaignAssignment?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeCampaignAssignment>> GetByCampaignAsync(int campaignId, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<EmployeeCampaignAssignment> assignments, CancellationToken ct = default);
    Task UpdateAsync(EmployeeCampaignAssignment assignment, CancellationToken ct = default);
    Task IncrementClickCountAsync(int assignmentId, CancellationToken ct = default);
    Task<int> CountByCampaignAsync(int campaignId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
