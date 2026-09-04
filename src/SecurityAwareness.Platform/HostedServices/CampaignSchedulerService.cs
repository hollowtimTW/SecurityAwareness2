using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Common.Enums;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Platform.HostedServices;

/// <summary>
/// Every minute, scans CampaignSchedules where IsActive AND NextRunAt <= UtcNow,
/// triggers dispatch for each, then updates LastRunAt + NextRunAt per ScheduleType.
/// </summary>
public class CampaignSchedulerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CampaignSchedulerService> _logger;

    public CampaignSchedulerService(IServiceScopeFactory scopeFactory, ILogger<CampaignSchedulerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CampaignSchedulerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSchedulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CampaignSchedulerService iteration failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (TaskCanceledException) { /* shutdown */ }
        }

        _logger.LogInformation("CampaignSchedulerService stopping.");
    }

    private async Task ProcessSchedulesAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AwarenessDbContext>();
        var campaigns = scope.ServiceProvider.GetRequiredService<ICampaignService>();
        var mailboxSvc = scope.ServiceProvider.GetRequiredService<ISenderMailboxService>();

        var now = DateTime.Now;
        var dueSchedules = await db.CampaignSchedules
            .Where(s => s.IsActive && s.NextRunAt != null && s.NextRunAt <= now)
            .ToListAsync(ct);

        if (dueSchedules.Count == 0) return;

        // Default mailbox: take all active
        var allMailboxes = await mailboxSvc.GetActiveAsync(ct);
        if (allMailboxes.Count == 0)
        {
            _logger.LogWarning("Due schedules found but no active mailboxes configured.");
            return;
        }
        var mailboxIds = allMailboxes.Select(m => m.SenderMailboxId).ToList();

        foreach (var sched in dueSchedules)
        {
            try
            {
                var queued = await campaigns.DispatchAsync(sched.CampaignId, mailboxIds, ct);
                _logger.LogInformation("Scheduler dispatched Campaign {Id} (sched={ScheduleId}, queued={N})",
                    sched.CampaignId, sched.ScheduleId, queued);

                sched.LastRunAt = now;
                sched.NextRunAt = ComputeNextRun(now, (ScheduleType)sched.ScheduleType,
                    sched.EndAt);
                if (sched.NextRunAt is null || (sched.EndAt.HasValue && sched.NextRunAt > sched.EndAt))
                {
                    sched.IsActive = false;
                }
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch schedule {Id}", sched.ScheduleId);
            }
        }
    }

    private static DateTime? ComputeNextRun(DateTime from, ScheduleType type, DateTime? endAt)
    {
        DateTime next = type switch
        {
            ScheduleType.OneTime => from.AddYears(100), // sentinel: never runs again
            ScheduleType.Daily => from.AddDays(1),
            ScheduleType.Weekly => from.AddDays(7),
            _ => from.AddYears(100)
        };
        if (endAt.HasValue && next > endAt.Value) return null;
        return next;
    }
}
