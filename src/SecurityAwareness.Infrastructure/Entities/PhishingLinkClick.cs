namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// One row per click — preserves IP and User-Agent for forensics.
/// </summary>
public class PhishingLinkClick
{
    public long ClickId { get; set; }
    public int AssignmentId { get; set; }
    public string TrackingToken { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime ClickedAt { get; set; }
    public byte LinkType { get; set; } // LinkType

    // Navigation
    public EmployeeCampaignAssignment? Assignment { get; set; }
}
