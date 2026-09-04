using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default);
    IQueryable<AuditLog> Query(
        string? entityType, string? actionName, string? actorKey,
        System.DateTime? fromUtc, System.DateTime? toUtc);
}
