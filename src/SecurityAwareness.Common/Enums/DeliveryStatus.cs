namespace SecurityAwareness.Common.Enums;

/// <summary>
/// Email delivery status (logged per dispatch attempt).
/// 0 = Pending, 1 = Sent, 2 = Failed, 3 = Throttled (rate-limited), 4 = Bounced.
/// </summary>
public enum DeliveryStatus : byte
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Throttled = 3,
    Bounced = 4
}
