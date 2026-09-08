using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class ScheduleController : Controller
{
    private readonly ICampaignScheduleService _schedules;
    private readonly ICampaignService _campaigns;

    public ScheduleController(ICampaignScheduleService schedules, ICampaignService campaigns)
    {
        _schedules = schedules;
        _campaigns = campaigns;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var all = await _schedules.GetAllAsync(ct);
        var campaignList = await _campaigns.GetAllAsync(ct);
        var campaignMap = campaignList.ToDictionary(c => c.CampaignId, c => c.Code);

        var vms = all.Select(s => new ScheduleListItemViewModel(
            s.ScheduleId,
            s.CampaignId,
            campaignMap.GetValueOrDefault(s.CampaignId, $"#{s.CampaignId}"),
            s.ScheduleType,
            s.StartAt,
            s.EndAt,
            s.NextRunAt,
            s.LastRunAt,
            s.IsActive)).ToList();
        return View(vms);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int? campaignId, CancellationToken ct)
    {
        var campaigns = await _campaigns.GetAllAsync(ct);
        ViewBag.Campaigns = campaigns;
        var vm = new ScheduleEditViewModel
        {
            CampaignId = campaignId ?? 0,
            StartAt = DateTime.Now.AddMinutes(5),
            IsActive = true
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(ScheduleEditViewModel vm, CancellationToken ct)
    {
        if (vm.EndAt.HasValue && vm.EndAt <= vm.StartAt)
        {
            ModelState.AddModelError("", "結束時間必須晚於開始時間");
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Campaigns = await _campaigns.GetAllAsync(ct);
            return View(vm);
        }

        var s = new CampaignSchedule
        {
            CampaignId = vm.CampaignId,
            ScheduleType = vm.ScheduleType,
            StartAt = vm.StartAt,
            EndAt = vm.EndAt
        };
        await _schedules.CreateAsync(s, ct);
        TempData["Message"] = $"已建立活動排程(下次執行:{s.NextRunAt:yyyy-MM-dd HH:mm})";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken ct)
    {
        var s = await _schedules.GetByIdAsync(id, ct);
        if (s is null) return NotFound();
        await _schedules.SetActiveAsync(id, !s.IsActive, ct);
        TempData["Message"] = $"已{(s.IsActive ? "停用" : "啟用")}排程";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _schedules.DeleteAsync(id, ct);
        TempData["Message"] = "已刪除排程";
        return RedirectToAction(nameof(Index));
    }
}
