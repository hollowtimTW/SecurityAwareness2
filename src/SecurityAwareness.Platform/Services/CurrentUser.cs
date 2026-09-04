using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SecurityAwareness.Platform.Services;

public interface ICurrentUser
{
    string? Username { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? Username => _accessor.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value;

    public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => _accessor.HttpContext?.User.IsInRole("Admin") ?? false;
}
