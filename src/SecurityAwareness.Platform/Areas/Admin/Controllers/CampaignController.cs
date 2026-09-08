using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;
using SecurityAwareness.Platform.Options;
using SecurityAwareness.Platform.ViewModels;
namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class CampaignController : Controller
{
    private readonly ICampaignService _service;
    private readonly IAssignmentRepository _assignments;
    private readonly ITrackingService _tracking;
    private readonly ISenderMailboxService _mailboxes;
    private readonly IOptions<TrackingOptions> _trackingOptions;

    public CampaignController(
        ICampaignService service,
        IAssignmentRepository assignments,
        ITrackingService tracking,
        ISenderMailboxService mailboxes,
        IOptions<TrackingOptions> trackingOptions)
    {
        _service = service;
        _assignments = assignments;
        _tracking = tracking;
        _mailboxes = mailboxes;
        _trackingOptions = trackingOptions;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _service.GetAllAsync(ct);
        var rows = new List<CampaignListItemViewModel>();
        foreach (var c in list)
        {
            var assignments = await _assignments.GetByCampaignAsync(c.CampaignId, ct);
            var totalClick = assignments.Sum(a => a.ClickCount);
            rows.Add(new CampaignListItemViewModel(
                c.CampaignId, c.Code, c.Title, c.StartAt, c.EndAt,
                StatusName(c.Status), assignments.Count, totalClick, c.CreatedAt));
        }
        return View(rows);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public IActionResult Create()
    {
        var now = DateTime.Now;
        var vm = new CampaignEditViewModel
        {
            StartAt = now.AddMinutes(1),
            EndAt = now.AddDays(7),
            BaseUrl = _trackingOptions.Value.BaseUrl
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create(CampaignEditViewModel vm, CancellationToken ct)
    {
        var actor = User.Identity?.Name ?? "system";
        var c = new PhishingCampaign
        {
            Code = vm.Code,
            Title = vm.Title,
            Description = vm.Description,
            StartAt = vm.StartAt,
            EndAt = vm.EndAt
        };
        try
        {
            await _service.CreateAsync(c, actor, ct);
            TempData["Message"] = $"活動 {c.Title} 已建立(狀態:Draft)";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Start(int id, CancellationToken ct)
    {
        await _service.StartAsync(id, ct);
        // Generate assignments + tracking URLs
        var baseUrl = _trackingOptions.Value.BaseUrl;
        var added = await _service.CreateAssignmentsAsync(id, baseUrl, ct);
        TempData["Message"] = $"活動已啟動,新增 {added} 個受測者指派";
        return RedirectToAction(nameof(Assignments), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> End(int id, CancellationToken ct)
    {
        await _service.EndAsync(id, ct);
        TempData["Message"] = "活動已結束";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Clone(int id, CancellationToken ct)
    {
        var source = await _service.GetByIdAsync(id, ct);
        if (source is null) return NotFound();
        var actor = User.Identity?.Name ?? "admin";
        var clone = new PhishingCampaign
        {
            Code = source.Code + "-COPY",
            Title = source.Title + " (副本)",
            Description = source.Description,
            StartAt = DateTime.Now.AddMinutes(1),
            EndAt = DateTime.Now.AddDays(7)
        };
        var created = await _service.CreateAsync(clone, actor, ct);
        TempData["Message"] = $"已複製為活動 #{created.CampaignId} {created.Code}";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Assignments(int id, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(id, ct);
        if (c is null) return NotFound();
        var list = await _assignments.GetByCampaignAsync(id, ct);
        var rows = list.Select(a => new AssignmentRow(
            a.AssignmentId,
            a.Employee?.EmployeeNo ?? "",
            a.Employee?.DisplayName ?? "",
            a.Employee?.Email ?? "",
            AssignmentStatusName(a.Status),
            a.ClickCount,
            a.DispatchedAt,
            a.FirstClickedAt
        )).ToList();

        var vm = new CampaignAssignmentsViewModel(
            c.CampaignId, c.Code, c.Title,
            TotalAssigned: list.Count,
            TotalClicked: list.Count(a => a.Status >= (byte)Common.Enums.AssignmentStatus.Clicked),
            TotalReported: list.Count(a => a.Status == (byte)Common.Enums.AssignmentStatus.Reported),
            rows);

        ViewBag.ReportUrl = Url.Action("ExportReport", "Import", new { campaignId = id });
        ViewBag.TrackingUrl = Url.Action("ExportTrackingLinks", "Import", new { campaignId = id });
        return View(vm);
    }

    private static string StatusName(byte status) => status switch
    {
        0 => "Draft",
        1 => "Active",
        2 => "Completed",
        3 => "Cancelled",
        _ => "?"
    };

    private static string AssignmentStatusName(byte status) => status switch
    {
        0 => "Pending",
        1 => "Dispatched",
        2 => "Clicked",
        3 => "Reported",
        _ => "?"
    };

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Dispatch(int id, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(id, ct);
        if (c is null) return NotFound();

        var allAssignments = await _assignments.GetByCampaignAsync(id, ct);
        var pending = allAssignments.Count(a => a.Status == (byte)Common.Enums.AssignmentStatus.Pending);

        var mbs = await _mailboxes.GetActiveAsync(ct);
        var options = mbs.Select(m => new MailboxOption(
            m.SenderMailboxId, m.Email, m.DisplayName, m.DailyQuota, m.DailyQuotaUsed, m.IsActive)).ToList();

        var vm = new DispatchViewModel(
            c.CampaignId, c.Code, c.Title, pending, options, options.Select(o => o.SenderMailboxId).ToList());
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Dispatch(int id, int[] mailboxIds, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(id, ct);
        if (c is null) return NotFound();
        try
        {
            var queued = await _service.DispatchAsync(id, mailboxIds, ct);
            TempData["Message"] = $"已將 {queued} 封信件加入寄信佇列";
            return RedirectToAction(nameof(Assignments), new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return await Dispatch(id, ct);
        }
    }
}
