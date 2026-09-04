namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// The pairing of one employee with one campaign. Holds the tracking token used
/// in the phishing URL, plus per-employee dispatch/click/reported timestamps and
/// a cached click count (incremented atomically per click).
/// </summary>
public class EmployeeCampaignAssignment
{
    public int AssignmentId { get; set; }
    public int CampaignId { get; set; }
    public int EmployeeId { get; set; }
    public string TrackingToken { get; set; } = string.Empty;
    public byte Status { get; set; } // AssignmentStatus
    public string? TokenUrl { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public DateTime? FirstClickedAt { get; set; }
    public DateTime? ReportedAt { get; set; }
    public int ClickCount { get; set; }

    // Navigation
    public PhishingCampaign? Campaign { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<PhishingLinkClick> Clicks { get; set; } = new List<PhishingLinkClick>();
}
