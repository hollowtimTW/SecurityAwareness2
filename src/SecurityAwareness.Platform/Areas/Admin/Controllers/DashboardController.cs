using Microsoft.AspNetCore.Mvc;

namespace SecurityAwareness.Platform.Areas.Admin.Controllers;

[Area("Admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin,Operator,Auditor")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
