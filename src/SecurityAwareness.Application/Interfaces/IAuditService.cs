using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface IAuditService
{
    Task WriteAsync(string action, string entityType, string? entityId, string actorUsername,
        string? detail = null, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default);

    Task<AuditLogPage> QueryAsync(AuditLogQuery query, CancellationToken ct = default);
}

public sealed record AuditLogQuery(
    string? EntityType,
    string? ActionName,    // renamed: avoid clashing with ASP.NET Core MVC route segment 'action'
    string? ActorKey,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Skip,
    int Take);

public sealed record AuditLogPage(
    IReadOnlyList<AuditLog> Items,
    int TotalCount,
    int Skip,
    int Take);
