using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface IClickRepository
{
    Task AddAsync(PhishingLinkClick click, CancellationToken ct = default);
    Task<IReadOnlyList<PhishingLinkClick>> GetByAssignmentAsync(int assignmentId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
