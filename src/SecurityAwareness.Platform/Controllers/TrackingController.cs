using Microsoft.AspNetCore.Mvc;
using SecurityAwareness.Application.Interfaces;

namespace SecurityAwareness.Platform.Controllers;

/// <summary>
/// Public-facing controller (no authentication required).
/// Employees click the tracking URL in the phishing email and land here.
/// </summary>
public class TrackingController : Controller
{
    private readonly ITrackingService _tracking;
    public TrackingController(ITrackingService tracking) => _tracking = tracking;

    [HttpGet]
    public async Task<IActionResult> Index(string token, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token)) return NotFound();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var ua = Request.Headers.UserAgent.ToString().Length > 500
            ? Request.Headers.UserAgent.ToString()[..500]
            : Request.Headers.UserAgent.ToString();

        await _tracking.RecordClickAsync(token, ip, ua, linkType: 1, ct);

        // Render disclosure page
        return View("Disclosure", token);
    }

    [HttpPost]
    public async Task<IActionResult> Report(string token, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(token)) return NotFound();
        await _tracking.MarkReportedAsync(token, ct);
        ViewBag.Reported = true;
        return View("Reported");
    }
}
