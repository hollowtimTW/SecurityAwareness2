using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Repositories;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class EmailDeliveryLogController : Controller
{
    private readonly IEmailDeliveryLogRepository _logs;
    private readonly ICampaignService _campaigns;
    private readonly ISenderMailboxService _mailboxes;
    private readonly IAssignmentRepository _assignments;

    private static readonly byte[] AllStatuses = { 0, 1, 2, 3 };

    public EmailDeliveryLogController(
        IEmailDeliveryLogRepository logs,
        ICampaignService campaigns,
        ISenderMailboxService mailboxes,
        IAssignmentRepository assignments)
    {
        _logs = logs;
        _campaigns = campaigns;
        _mailboxes = mailboxes;
        _assignments = assignments;
    }

    public async Task<IActionResult> Index(
        int? campaignId, byte? status, int? mailboxId, int page = 1, CancellationToken ct = default)
    {
        var q = new EmailDeliveryQuery(campaignId, status, mailboxId, null, null, page, 50);
        var result = await _logs.QueryAsync(q, ct);

        var rows = result.Items.Select(l => new EmailDeliveryRow(
            LogId: l.LogId,
            CreatedAt: l.CreatedAt,
            SentAt: l.SentAt,
            Subject: l.Subject,
            ToEmail: l.Assignment?.Employee?.Email ?? "(deleted)",
            ToDisplayName: l.Assignment?.Employee?.DisplayName ?? "",
            CampaignCode: l.Assignment?.Campaign?.Code,
            SenderMailboxEmail: l.SenderMailbox?.Email ?? "",
            Status: StatusName(l.Status),
            ErrorDetail: l.ErrorDetail,
            AttemptCount: l.AttemptCount
        )).ToList();

        var allCampaigns = await _campaigns.GetAllAsync(ct);
        var activeMailboxes = await _mailboxes.GetActiveAsync(ct);
        var campaignOpts = allCampaigns.Select(c => new CampaignOption(c.CampaignId, c.Code, c.Title)).ToList();
        var mailboxOpts = activeMailboxes.Select(m => new MailboxOption(
            m.SenderMailboxId, m.Email, m.DisplayName, m.DailyQuota, m.DailyQuotaUsed, m.IsActive)).ToList();

        var vm = new EmailDeliveryPageViewModel(
            Items: rows,
            TotalCount: result.TotalCount,
            Page: result.Page,
            PageSize: result.PageSize,
            TotalPages: Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)result.PageSize)),
            CampaignId: campaignId,
            Status: status,
            MailboxId: mailboxId,
            Campaigns: campaignOpts,
            Mailboxes: mailboxOpts,
            StatusOptions: AllStatuses.Select(s => new SelectOption(s, StatusName(s))).ToList());

        return View(vm);
    }

    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var log = await _logs.GetByIdAsync(id, ct);
        if (log is null) return NotFound();

        ViewBag.Status = StatusName(log.Status);
        ViewBag.SentAt = log.SentAt;
        ViewBag.CreatedAt = log.CreatedAt;
        ViewBag.AttemptCount = log.AttemptCount;
        ViewBag.Error = log.ErrorDetail;
        ViewBag.Subject = log.Subject;
        ViewBag.Mailbox = log.SenderMailbox?.Email ?? "(deleted)";
        ViewBag.ToEmail = log.Assignment?.Employee?.Email ?? "(deleted)";
        ViewBag.ToName = log.Assignment?.Employee?.DisplayName ?? "";
        ViewBag.CampaignCode = log.Assignment?.Campaign?.Code ?? "";
        ViewBag.CampaignTitle = log.Assignment?.Campaign?.Title ?? "";
        ViewBag.ProviderMessageId = log.ProviderMessageId;
        ViewBag.AssignmentId = log.AssignmentId;
        ViewBag.TrackingUrl = log.Assignment?.TokenUrl;
        return View(log);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Resend(long id, CancellationToken ct)
    {
        var log = await _logs.GetByIdAsync(id, ct);
        if (log is null) return NotFound();
        if (log.Status == 0) // Success — don't resend
        {
            TempData["Error"] = "此信件已成功,無需重寄";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Force the assignment back to Pending so Dispatch will pick it up again
        var assignment = log.Assignment;
        if (assignment is not null)
        {
            assignment.Status = (byte)Common.Enums.AssignmentStatus.Pending;
            await _assignments.UpdateAsync(assignment, ct);
            await _assignments.SaveChangesAsync(ct);
        }

        TempData["Message"] = "已將此信件的 assignment 重設為 Pending,下次 Dispatch 會重寄";
        return RedirectToAction(nameof(Details), new { id });
    }

    private static string StatusName(byte status) => status switch
    {
        0 => "Success",
        1 => "Failed",
        2 => "Retry",
        3 => "Pending",
        _ => "?"
    };
}
