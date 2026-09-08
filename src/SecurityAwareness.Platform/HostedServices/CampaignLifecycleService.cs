using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Platform.HostedServices;

/// <summary>
/// Runs every 5 minutes. Automatically ends campaigns whose EndAt has passed.
/// </summary>
public class CampaignLifecycleService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignLifecycleService> _logger;

    public CampaignLifecycleService(IServiceScopeFactory scopeFactory, ILogger<CampaignLifecycleService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CampaignLifecycleService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await AutoEndExpiredAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Auto-end iteration failed"); }

            try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task AutoEndExpiredAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AwarenessDbContext>();
        var now = DateTime.Now;
        var expired = await db.PhishingCampaigns
            .Where(c => c.Status == 1 && c.EndAt < now)
            .ToListAsync(ct);
        foreach (var c in expired)
        {
            c.Status = (byte)2; // Completed
            c.UpdatedAt = now;
            _logger.LogInformation("Auto-ended Campaign {Id} ({Code}) — EndAt={EndAt}",
                c.CampaignId, c.Code, c.EndAt);
        }
        if (expired.Count > 0) await db.SaveChangesAsync(ct);
    }
}
