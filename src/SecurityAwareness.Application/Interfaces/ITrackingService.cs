using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface ITrackingService
{
    /// <summary>
    /// Record a click on a phishing link. Updates ClickCount and Status atomically.
    /// Returns the assignment if found, else null.
    /// </summary>
    Task<EmployeeCampaignAssignment?> RecordClickAsync(
        string token, string? ipAddress, string? userAgent, byte linkType,
        CancellationToken ct = default);

    /// <summary>
    /// Mark an assignment as Reported by the employee.
    /// </summary>
    Task<bool> MarkReportedAsync(string token, CancellationToken ct = default);
}
