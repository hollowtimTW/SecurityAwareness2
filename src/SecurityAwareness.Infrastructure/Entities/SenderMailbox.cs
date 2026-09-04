namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// A mailbox usable for sending phishing simulations. Daily quota spreads load
/// across multiple mailboxes to avoid per-mailbox throttling.
/// </summary>
public class SenderMailbox
{
    public int SenderMailboxId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public byte Provider { get; set; } // EmailProvider
    public int DailyQuota { get; set; } = 500;
    public int DailyQuotaUsed { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
