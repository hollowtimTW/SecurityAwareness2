using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class TrackingService : ITrackingService
{
    private readonly IAssignmentRepository _assignments;
    private readonly IClickRepository _clicks;
    private readonly IAuditService _audit;
    private readonly ILogger<TrackingService> _logger;

    public TrackingService(
        IAssignmentRepository assignments,
        IClickRepository clicks,
        IAuditService audit,
        ILogger<TrackingService> logger)
    {
        _assignments = assignments;
        _clicks = clicks;
        _audit = audit;
        _logger = logger;
    }

    public async Task<EmployeeCampaignAssignment?> RecordClickAsync(
        string token, string? ipAddress, string? userAgent, byte linkType,
        CancellationToken ct = default)
    {
        var assignment = await _assignments.GetByTokenAsync(token, ct);
        if (assignment is null)
        {
            _logger.LogWarning("Click with unknown token {TokenPrefix}", token[..Math.Min(8, token.Length)]);
            return null;
        }

        // 1. Append to detailed click log
        var click = new PhishingLinkClick
        {
            AssignmentId = assignment.AssignmentId,
            TrackingToken = token,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ClickedAt = DateTime.Now,
            LinkType = linkType
        };
        await _clicks.AddAsync(click, ct);
        await _clicks.SaveChangesAsync(ct);

        // 2. Atomically increment ClickCount, set FirstClickedAt, set Status=Clicked
        await _assignments.IncrementClickCountAsync(assignment.AssignmentId, ct);

        await _audit.WriteAsync("Click", "Assignment", assignment.AssignmentId.ToString(),
            $"employee#{assignment.EmployeeId}", $"ip={ipAddress}", ct);

        _logger.LogInformation("Recorded click on Assignment {Id} (employee {EmployeeId})",
            assignment.AssignmentId, assignment.EmployeeId);
        return assignment;
    }

    public async Task<bool> MarkReportedAsync(string token, CancellationToken ct = default)
    {
        var assignment = await _assignments.GetByTokenAsync(token, ct);
        if (assignment is null) return false;

        if (assignment.ReportedAt is null)
        {
            assignment.Status = (byte)3; // Reported
            assignment.ReportedAt = DateTime.Now;
            await _assignments.UpdateAsync(assignment, ct);
            await _assignments.SaveChangesAsync(ct);
        }
        await _audit.WriteAsync("Report", "Assignment", assignment.AssignmentId.ToString(),
            $"employee#{assignment.EmployeeId}", null, ct);
        return true;
    }
}
