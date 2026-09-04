using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class ReportController : Controller
{
    private readonly AwarenessDbContext _db;
    private readonly ICampaignService _campaigns;

    public ReportController(AwarenessDbContext db, ICampaignService campaigns)
    {
        _db = db;
        _campaigns = campaigns;
    }

    public async Task<IActionResult> Index(int? campaignId, CancellationToken ct)
    {
        var allCampaigns = await _campaigns.GetAllAsync(ct);

        var target = campaignId.HasValue
            ? allCampaigns.FirstOrDefault(c => c.CampaignId == campaignId.Value)
            : allCampaigns.FirstOrDefault();

        if (target is null)
        {
            ViewBag.Message = "尚無活動可供統計";
            return View(new ReportDashboardViewModel());
        }

        var assignments = await _db.EmployeeCampaignAssignments.AsNoTracking()
            .Where(a => a.CampaignId == target.CampaignId)
            .ToListAsync(ct);

        var totalAssigned = assignments.Count;
        var clickedCount = assignments.Count(a => a.Status >= 2);
        var reportedCount = assignments.Count(a => a.Status == 3);

        var clickRate = totalAssigned == 0 ? 0 : (double)clickedCount / totalAssigned * 100;
        var reportRate = totalAssigned == 0 ? 0 : (double)reportedCount / totalAssigned * 100;

        // Department metrics — load raw, group in memory
        var allAssignments = await _db.EmployeeCampaignAssignments.AsNoTracking()
            .Where(a => a.CampaignId == target.CampaignId)
            .ToListAsync(ct);
        var employees = await _db.Employees.AsNoTracking()
            .Where(e => allAssignments.Select(a => a.EmployeeId).Contains(e.EmployeeId))
            .ToDictionaryAsync(e => e.EmployeeId, ct);
        var depts = await _db.Departments.AsNoTracking()
            .Where(d => employees.Values.Select(e => e.DepartmentId).Contains(d.DepartmentId))
            .ToDictionaryAsync(d => d.DepartmentId, ct);

        var deptGroups = allAssignments
            .GroupBy(a => employees.TryGetValue(a.EmployeeId, out var e) ? e.DepartmentId : 0)
            .Where(g => g.Key != 0 && depts.ContainsKey(g.Key))
            .Select(g => new DepartmentMetric(
                depts[g.Key].Code,
                depts[g.Key].Name,
                g.Count(),
                g.Count(a => a.Status >= 2),
                g.Count() == 0 ? 0 : (double)g.Count(a => a.Status >= 2) / g.Count() * 100
            ))
            .OrderByDescending(m => m.ClickRate)
            .ToList();

        // Hourly click distribution
        var assignmentIds = allAssignments.Select(a => a.AssignmentId).ToHashSet();
        var clickRows = await _db.PhishingLinkClicks.AsNoTracking()
            .Where(c => assignmentIds.Contains(c.AssignmentId))
            .Select(c => c.ClickedAt.Hour)
            .ToListAsync(ct);
        var hourly = Enumerable.Range(0, 24).Select(h => new HourlyClick(h,
            clickRows.Count(hr => hr == h))).ToList();

        var vm = new ReportDashboardViewModel
        {
            CampaignId = target.CampaignId,
            CampaignCode = target.Code,
            CampaignTitle = target.Title,
            TotalAssigned = totalAssigned,
            ClickedCount = clickedCount,
            ReportedCount = reportedCount,
            ClickRate = Math.Round(clickRate, 1),
            ReportRate = Math.Round(reportRate, 1),
            Departments = deptGroups,
            HourlyClicks = hourly
        };
        ViewBag.AllCampaigns = allCampaigns;
        return View(vm);
    }
}
