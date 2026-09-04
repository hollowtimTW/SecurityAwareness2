namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// Backend operator-action audit log. Written by AuditActionFilter on every controller action.
/// </summary>
public class AuditLog
{
    public long AuditLogId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string ActorUsername { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public DateTime OccurredAt { get; set; }
}
