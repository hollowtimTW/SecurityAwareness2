namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// Schedule for automatic campaign dispatch (OneTime, Daily, Weekly).
/// BackgroundService scans these and triggers dispatch when NextRunAt arrives.
/// </summary>
public class CampaignSchedule
{
    public int ScheduleId { get; set; }
    public int CampaignId { get; set; }
    public byte ScheduleType { get; set; } = 1; // ScheduleType
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime? NextRunAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Navigation
    public PhishingCampaign? Campaign { get; set; }
}
