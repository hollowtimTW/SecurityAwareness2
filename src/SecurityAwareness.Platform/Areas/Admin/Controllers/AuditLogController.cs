using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Platform.Filters;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Auditor")]
[ServiceFilter(typeof(AuditActionFilter))]
public class AuditLogController : Controller
{
    private const int PageSize = 50;
    private readonly IAuditService _audit;

    public AuditLogController(IAuditService audit) => _audit = audit;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? entityType,
        string? actionName,
        string? actorKey,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page = 1,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        var skip = (page - 1) * PageSize;

        var query = new AuditLogQuery(
            EntityType: entityType,
            ActionName: actionName,
            ActorKey: actorKey,
            FromUtc: fromUtc,
            ToUtc: toUtc,
            Skip: skip,
            Take: PageSize);

        var result = await _audit.QueryAsync(query, ct);

        var items = result.Items.Select(a => new AuditLogItemViewModel(
            a.AuditLogId, a.Action, a.EntityType, a.EntityId,
            a.ActorUsername, a.OccurredAt, a.Detail)).ToList();

        var totalPages = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)PageSize));

        var vm = new AuditLogPageViewModel(items, result.TotalCount, page, PageSize, totalPages,
            entityType, actionName, actorKey, fromUtc, toUtc);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var list = await _audit.GetRecentAsync(1000, ct);
        var entry = list.FirstOrDefault(a => a.AuditLogId == id);
        if (entry is null) return NotFound();
        ViewBag.Detail = entry.Detail;
        ViewBag.Id = entry.AuditLogId;
        return View(entry);
    }
}
