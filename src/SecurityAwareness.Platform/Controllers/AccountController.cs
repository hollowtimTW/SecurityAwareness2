using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;

namespace SecurityAwareness.Platform.Controllers;

public class AccountController : Controller
{
    private readonly ISystemAccountService _accountService;
    public AccountController(ISystemAccountService accountService) => _accountService = accountService;

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
        => View(new SecurityAwareness.Platform.ViewModels.LoginViewModel(returnUrl, null));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl)
    {
        var account = await _accountService.AuthenticateAsync(username, password, HttpContext.RequestAborted);
        if (account is null)
        {
            return View(new SecurityAwareness.Platform.ViewModels.LoginViewModel(returnUrl, "帳號或密碼錯誤"));
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.Name, account.Username),
            new(System.Security.Claims.ClaimTypes.NameIdentifier, account.AccountId.ToString()),
        };
        var roleName = account.Role switch
        {
            0 => "Admin",
            1 => "Operator",
            2 => "Auditor",
            _ => "Auditor"
        };
        claims.Add(new(System.Security.Claims.ClaimTypes.Role, roleName));

        var identity = new System.Security.Claims.ClaimsIdentity(claims,
            Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
