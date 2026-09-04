namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// One row per email dispatch attempt for one (assignment, mailbox) pair.
/// </summary>
public class EmailDeliveryLog
{
    public long LogId { get; set; }
    public int AssignmentId { get; set; }
    public int SenderMailboxId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public byte Status { get; set; } // DeliveryStatus
    public string? ProviderMessageId { get; set; }
    public string? ErrorDetail { get; set; }
    public int AttemptCount { get; set; } = 1;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
