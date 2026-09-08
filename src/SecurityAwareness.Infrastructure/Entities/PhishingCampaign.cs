namespace SecurityAwareness.Infrastructure.Entities;

/// <summary>
/// A phishing simulation campaign — one run targeting many employees over a date range.
/// </summary>
public class PhishingCampaign
{
    public int CampaignId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public byte Status { get; set; } // CampaignStatus
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// JSON array of department ids targeted by this campaign.
    /// Null or empty = all active employees.
    /// </summary>
    public string? TargetDepartmentIds { get; set; }

    // Navigation
    public ICollection<EmployeeCampaignAssignment> Assignments { get; set; } = new List<EmployeeCampaignAssignment>();
}
