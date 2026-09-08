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
    private readonly IEmployeeService _employees;

    public DepartmentController(IDepartmentService service, IEmployeeService employees)
    {
        _service = service;
        _employees = employees;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _service.GetAllAsync(ct);
        return View(list.Select(d => new DepartmentListItemViewModel(d.DepartmentId, d.Code, d.Name, d.IsActive)).ToList());
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new DepartmentEditViewModel
        {
            IsActive = true
        };
        await LoadSelectsAsync(vm, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(DepartmentEditViewModel vm, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vm.Code) || string.IsNullOrWhiteSpace(vm.Name))
        {
            ModelState.AddModelError("", "代碼與名稱為必填");
            await LoadSelectsAsync(vm, ct);
            return View(vm);
        }
        var d = new Department
        {
            Code = vm.Code,
            Name = vm.Name,
            IsActive = vm.IsActive,
            ManagerEmployeeId = vm.ManagerEmployeeId
        };
        await _service.CreateAsync(d, ct);
        TempData["Message"] = $"部門 {vm.Name} 已新增";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var d = await _service.GetByIdAsync(id, ct);
        if (d is null) return NotFound();
        var vm = new DepartmentEditViewModel
        {
            DepartmentId = d.DepartmentId,
            Code = d.Code,
            Name = d.Name,
            IsActive = d.IsActive,
            ManagerEmployeeId = d.ManagerEmployeeId
        };
        await LoadSelectsAsync(vm, ct);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id, DepartmentEditViewModel vm, CancellationToken ct)
    {
        var d = await _service.GetByIdAsync(id, ct);
        if (d is null) return NotFound();
        d.Code = vm.Code;
        d.Name = vm.Name;
        d.IsActive = vm.IsActive;
        d.ManagerEmployeeId = vm.ManagerEmployeeId;
        await _service.UpdateAsync(d, ct);
        TempData["Message"] = $"部門 {vm.Name} 已更新";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectsAsync(DepartmentEditViewModel vm, CancellationToken ct)
    {
        var depts = await _service.GetAllAsync(ct);
        vm.Departments = depts.Select(d => new DepartmentOption(d.DepartmentId, d.Code, d.Name)).ToList();

        var employees = await _employees.GetAllAsync(ct);
        vm.Managers = employees.Where(e => e.IsActive)
            .OrderBy(e => e.EmployeeNo)
            .Select(e => new ManagerOption(e.EmployeeId, e.EmployeeNo, e.DisplayName))
            .ToList();
    }
}
