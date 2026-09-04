using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class EmployeeController : Controller
{
    private readonly IEmployeeService _employees;
    private readonly IDepartmentService _departments;

    public EmployeeController(IEmployeeService employees, IDepartmentService departments)
    {
        _employees = employees;
        _departments = departments;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var employees = await _employees.GetAllAsync(ct);
        var items = employees.Select(e => new EmployeeListItemViewModel(
            e.EmployeeId, e.EmployeeNo, e.Email, e.DisplayName,
            e.Department?.Name ?? "-", e.IsActive, e.CreatedAt)).ToList();
        return View(items);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var depts = await _departments.GetAllAsync(ct);
        ViewBag.Departments = depts.Select(d => new DepartmentOption(d.DepartmentId, d.Code, d.Name)).ToList();
        return View(new EmployeeEditViewModel { Departments = ViewBag.Departments });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Create(EmployeeEditViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var depts = await _departments.GetAllAsync(ct);
            vm.Departments = depts.Select(d => new DepartmentOption(d.DepartmentId, d.Code, d.Name)).ToList();
            return View(vm);
        }
        var emp = new SecurityAwareness.Infrastructure.Entities.Employee
        {
            EmployeeNo = vm.EmployeeNo,
            Email = vm.Email,
            DisplayName = vm.DisplayName,
            DepartmentId = vm.DepartmentId,
            IsActive = vm.IsActive
        };
        await _employees.CreateAsync(emp, ct);
        TempData["Message"] = $"員工 {emp.DisplayName} 已新增";
        return RedirectToAction(nameof(Index));
    }
}
