using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Common.Enums;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class SenderMailboxController : Controller
{
    private readonly ISenderMailboxService _service;
    public SenderMailboxController(ISenderMailboxService service) => _service = service;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var all = await _service.GetAllAsync(ct);
        var items = all.Select(m => new MailboxListItemViewModel(
            m.SenderMailboxId, m.Email, m.DisplayName, m.Provider,
            m.DailyQuota, m.DailyQuotaUsed, m.IsActive)).ToList();
        return View(items);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        var vm = new MailboxEditViewModel();
        vm.Providers = new List<SelectOption>
        {
            new(0, "Fake (寫 .eml 到本機)"),
            new(1, "Graph (M365)"),
            new(2, "SMTP")
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(MailboxEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Providers = new List<SelectOption>
            {
                new(0, "Fake (寫 .eml 到本機)"),
                new(1, "Graph (M365)"),
                new(2, "SMTP")
            };
            return View(vm);
        }
        var m = new SenderMailbox
        {
            Email = vm.Email,
            DisplayName = vm.DisplayName,
            Provider = vm.Provider,
            DailyQuota = vm.DailyQuota
        };
        await _service.CreateAsync(m, ct);
        TempData["Message"] = $"寄件信箱 {m.Email} 已新增";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var m = await _service.GetByIdAsync(id, ct);
        if (m is null) return NotFound();
        var vm = new MailboxEditViewModel
        {
            SenderMailboxId = m.SenderMailboxId,
            Email = m.Email,
            DisplayName = m.DisplayName,
            Provider = m.Provider,
            DailyQuota = m.DailyQuota,
            DailyQuotaUsed = m.DailyQuotaUsed,
            IsActive = m.IsActive,
            Providers = new List<SelectOption>
            {
                new(0, "Fake (寫 .eml 到本機)"),
                new(1, "Graph (M365)"),
                new(2, "SMTP")
            }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(MailboxEditViewModel vm, CancellationToken ct)
    {
        var m = await _service.GetByIdAsync(vm.SenderMailboxId, ct);
        if (m is null) return NotFound();
        m.Email = vm.Email;
        m.DisplayName = vm.DisplayName;
        m.Provider = vm.Provider;
        m.DailyQuota = vm.DailyQuota;
        m.IsActive = vm.IsActive;
        await _service.UpdateAsync(m, ct);
        TempData["Message"] = "已更新";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken ct)
    {
        var m = await _service.GetByIdAsync(id, ct);
        if (m is null) return NotFound();
        await _service.SetActiveAsync(id, !m.IsActive, ct);
        TempData["Message"] = $"已{(m.IsActive ? "停用" : "啟用")} {m.Email}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        TempData["Message"] = "已刪除寄件信箱";
        return RedirectToAction(nameof(Index));
    }
}
