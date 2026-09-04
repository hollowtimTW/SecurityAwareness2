using System.Text;
using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;
using SecurityAwareness.Platform.ViewModels;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator")]
public class ImportController : Controller
{
    private readonly IEmployeeService _employees;
    private readonly IDepartmentService _departments;
    private readonly ICampaignService _campaigns;
    private readonly IAssignmentRepository _assignments;
    private readonly ITrackingService _tracking;

    public ImportController(
        IEmployeeService employees,
        IDepartmentService departments,
        ICampaignService campaigns,
        IAssignmentRepository assignments,
        ITrackingService tracking)
    {
        _employees = employees;
        _departments = departments;
        _campaigns = campaigns;
        _assignments = assignments;
        _tracking = tracking;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public IActionResult DownloadSample()
    {
        var csv = "EmployeeNo,DisplayName,Email,DepartmentCode,IsActive\r\nE001,王小明,ming@egit.com.tw,SYS,1\r\nE002,林大嬸,lin@egit.com.tw,PROJ,1\r\n";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "employees-sample.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEmployees(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "請選擇檔案";
            return RedirectToAction(nameof(Index));
        }

        using var sr = new StreamReader(file.OpenReadStream());
        var csv = await sr.ReadToEndAsync(ct);
        var rows = CsvHelper.Parse(csv);
        if (rows.Count < 2) { TempData["Error"] = "CSV 至少要 header + 1 列"; return RedirectToAction(nameof(Index)); }

        var depts = await _departments.GetAllAsync(ct);
        var deptMap = depts.ToDictionary(d => d.Code, d => d.DepartmentId);

        var header = rows[0];
        int idxNo = header.IndexOf("EmployeeNo");
        int idxName = header.IndexOf("DisplayName");
        int idxEmail = header.IndexOf("Email");
        int idxDept = header.IndexOf("DepartmentCode");
        int idxActive = header.IndexOf("IsActive");

        if (idxNo < 0 || idxName < 0 || idxEmail < 0 || idxDept < 0)
        {
            TempData["Error"] = "CSV header 缺欄位,需包含 EmployeeNo,DisplayName,Email,DepartmentCode";
            return RedirectToAction(nameof(Index));
        }

        int success = 0, fail = 0;
        var errors = new List<string>();
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            try
            {
                var code = row[idxDept];
                if (!deptMap.TryGetValue(code, out var deptId))
                {
                    errors.Add($"row {r + 1}: 部門代碼 '{code}' 不存在"); fail++; continue;
                }
                var emp = new Employee
                {
                    EmployeeNo = row[idxNo],
                    DisplayName = row[idxName],
                    Email = row[idxEmail],
                    DepartmentId = deptId,
                    IsActive = idxActive < 0 || row[idxActive] != "0"
                };
                await _employees.CreateAsync(emp, ct);
                success++;
            }
            catch (Exception ex)
            {
                errors.Add($"row {r + 1}: {ex.Message}");
                fail++;
            }
        }
        TempData["Message"] = $"匯入完成:成功 {success},失敗 {fail}。" + (errors.Count > 0 ? $" 錯誤: {string.Join("; ", errors.Take(5))}" : "");
        return RedirectToAction("Index", "Employee", new { area = "Admin" });
    }

    [HttpGet]
    public async Task<IActionResult> ExportTrackingLinks(int campaignId, CancellationToken ct)
    {
        var assignments = await _assignments.GetByCampaignAsync(campaignId, ct);
        if (assignments.Count == 0)
        {
            TempData["Error"] = "該活動尚無指派資料";
            return RedirectToAction("Assignments", "Campaign", new { id = campaignId });
        }
        var sb = new StringBuilder();
        sb.AppendLine("AssignmentId,EmployeeNo,DisplayName,Email,Status,TrackingToken,TokenUrl");
        foreach (var a in assignments)
        {
            var e = a.Employee;
            sb.AppendLine(string.Join(",", new[]
            {
                a.AssignmentId.ToString(),
                CsvHelper.Escape(e?.EmployeeNo ?? ""),
                CsvHelper.Escape(e?.DisplayName ?? ""),
                CsvHelper.Escape(e?.Email ?? ""),
                a.Status.ToString(),
                a.TrackingToken,
                CsvHelper.Escape(a.TokenUrl ?? "")
            }));
        }
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv; charset=utf-8", $"tracking-links-{campaignId}.csv");
    }
}
