using Microsoft.AspNetCore.Mvc.Filters;
using SecurityAwareness.Application.Interfaces;

namespace SecurityAwareness.Platform.Filters;

/// <summary>
/// Writes an AuditLog row on every successful controller action in the Admin area.
/// </summary>
public class AuditActionFilter : IAsyncActionFilter
{
    private readonly IAuditService _audit;

    public AuditActionFilter(IAuditService audit) => _audit = audit;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Exception != null || executed.Canceled) return;

        var user = context.HttpContext.User.Identity?.Name ?? "anonymous";
        var area = context.RouteData.Values["area"]?.ToString();
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        var entityId = context.RouteData.Values["id"]?.ToString();

        if (string.IsNullOrEmpty(controller)) return;

        // Only audit Admin-area controllers
        if (area != "Admin") return;

        var entityType = controller!;
        var auditAction = action ?? "Unknown";

        await _audit.WriteAsync(auditAction, entityType, entityId, user, ct: context.HttpContext.RequestAborted);
    }
}
