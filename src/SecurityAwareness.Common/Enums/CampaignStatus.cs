namespace SecurityAwareness.Common.Enums;

/// <summary>
/// Status of a phishing campaign.
/// 0 = Draft (admin only), 1 = Active (dispatching), 2 = Completed, 3 = Cancelled.
/// </summary>
public enum CampaignStatus : byte
{
    Draft = 0,
    Active = 1,
    Completed = 2,
    Cancelled = 3
}
