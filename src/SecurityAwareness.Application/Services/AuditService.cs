using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repo;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository repo, ILogger<AuditService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task WriteAsync(string action, string entityType, string? entityId, string actorUsername,
        string? detail = null, CancellationToken ct = default)
    {
        try
        {
            var entry = new AuditLog
            {
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                ActorUsername = string.IsNullOrWhiteSpace(actorUsername) ? "anonymous" : actorUsername,
                Detail = detail,
                OccurredAt = DateTime.Now
            };
            await _repo.AddAsync(entry, ct);
            await _repo.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log {Action}/{EntityType}/{EntityId}",
                action, entityType, entityId);
        }
    }

    public async Task<AuditLogPage> QueryAsync(AuditLogQuery query, CancellationToken ct = default)
    {
        var q = _repo.Query(query.EntityType, query.ActionName, query.ActorKey, query.FromUtc, query.ToUtc);
        var total = q.Count();
        var items = q.OrderByDescending(a => a.OccurredAt)
            .Skip(query.Skip).Take(query.Take)
            .ToList();
        return new AuditLogPage(items, total, query.Skip, query.Take);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default)
    {
        var list = await _repo.Query(null, null, null, null, null)
            .OrderByDescending(a => a.OccurredAt).Take(take).ToListAsync(ct);
        return list;
    }
}
