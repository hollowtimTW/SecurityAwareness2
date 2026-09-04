using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AwarenessDbContext _db;
    public AuditLogRepository(AwarenessDbContext db) => _db = db;

    public async Task AddAsync(AuditLog entry, CancellationToken ct = default)
        => await _db.AuditLogs.AddAsync(entry, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default)
        => await _db.AuditLogs.AsNoTracking().OrderByDescending(a => a.OccurredAt).Take(take).ToListAsync(ct);

    public IQueryable<AuditLog> Query(
        string? entityType, string? actionName, string? actorKey,
        System.DateTime? fromUtc, System.DateTime? toUtc)
    {
        var q = _db.AuditLogs.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(entityType)) q = q.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrEmpty(actionName)) q = q.Where(a => a.Action == actionName);
        if (!string.IsNullOrEmpty(actorKey)) q = q.Where(a => a.ActorUsername == actorKey);
        if (fromUtc.HasValue) q = q.Where(a => a.OccurredAt >= fromUtc.Value);
        if (toUtc.HasValue) q = q.Where(a => a.OccurredAt <= toUtc.Value);
        return q;
    }
}
