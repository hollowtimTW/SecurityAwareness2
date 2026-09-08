using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Platform.HostedServices;

/// <summary>
/// Runs at midnight (and on startup if last reset was &gt; 20h ago).
/// Resets DailyQuotaUsed to 0 on all sender mailboxes.
/// </summary>
public class QuotaResetService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<QuotaResetService> _logger;

    public QuotaResetService(IServiceScopeFactory scopeFactory, ILogger<QuotaResetService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QuotaResetService started.");

        // Run on startup (idempotent)
        await TryResetAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var nextMidnight = DateTime.Today.AddDays(1);
                var delay = nextMidnight - DateTime.Now;
                if (delay < TimeSpan.Zero) delay = TimeSpan.FromHours(1);
                await Task.Delay(delay, stoppingToken);
                await TryResetAsync(stoppingToken);
            }
            catch (TaskCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QuotaResetService iteration failed");
            }
        }
    }

    private async Task TryResetAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISenderMailboxRepository>();
        try
        {
            await repo.ResetAllDailyQuotasAsync(ct);
            _logger.LogInformation("Quota reset successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Quota reset failed");
        }
    }
}
