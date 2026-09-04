using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _service;
    public DepartmentController(IDepartmentService service) => _service = service;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _service.GetAllAsync(ct);
        return View(list.Select(d => new DepartmentListItemViewModel(d.DepartmentId, d.Code, d.Name, d.IsActive)).ToList());
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public IActionResult Create() => View(new DepartmentListItemViewModel(0, "", "", true));

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(string code, string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("", "代碼與名稱為必填");
            return View(new DepartmentListItemViewModel(0, code, name, true));
        }
        await _service.CreateAsync(new Department { Code = code, Name = name }, ct);
        TempData["Message"] = $"部門 {name} 已新增";
        return RedirectToAction(nameof(Index));
    }
}
