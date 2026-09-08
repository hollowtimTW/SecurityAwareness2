using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Platform.HostedServices;

/// <summary>
/// Pulls EmailMessage objects off the queue channel and dispatches them via the configured IEmailDeliveryService.
/// Updates EmailDeliveryLog + Assignment.DispatchedAt on success.
/// </summary>
public class EmailDispatcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailQueueChannel _queue;
    private readonly ILogger<EmailDispatcherService> _logger;

    public EmailDispatcherService(
        IServiceScopeFactory scopeFactory,
        IEmailQueueChannel queue,
        ILogger<EmailDispatcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailDispatcherService started.");

        await foreach (var msg in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var delivery = scope.ServiceProvider.GetRequiredService<IEmailDeliveryService>();
                var logRepo = scope.ServiceProvider.GetRequiredService<IEmailDeliveryLogRepository>();
                var assignmentRepo = scope.ServiceProvider.GetRequiredService<IAssignmentRepository>();
                var mailboxRepo = scope.ServiceProvider.GetRequiredService<ISenderMailboxRepository>();

                var outcome = await delivery.SendAsync(msg, stoppingToken);

                // Persist EmailDeliveryLog
                var mailbox = await mailboxRepo.GetByIdAsync(msg.MailboxId, stoppingToken);
                var log = new EmailDeliveryLog
                {
                    AssignmentId = msg.AssignmentId,
                    SenderMailboxId = msg.MailboxId,
                    Subject = msg.Subject,
                    Status = (byte)(outcome.Success ? 0 : 1), // 0=Success, 1=Failed
                    ProviderMessageId = outcome.Success ? msg.MessageId : null,
                    ErrorDetail = outcome.Error,
                    AttemptCount = 1,
                    SentAt = outcome.Success ? DateTime.UtcNow : null,
                    CreatedAt = DateTime.UtcNow
                };
                await logRepo.AddAsync(log, stoppingToken);
                await logRepo.SaveChangesAsync(stoppingToken);

                // Update Assignment status + DispatchedAt on success
                if (outcome.Success && mailbox is not null)
                {
                    var assignment = await assignmentRepo.GetByIdAsync(msg.AssignmentId, stoppingToken);
                    if (assignment is not null)
                    {
                        assignment.Status = (byte)Common.Enums.AssignmentStatus.Dispatched;
                        assignment.DispatchedAt = DateTime.Now;
                        await assignmentRepo.UpdateAsync(assignment, stoppingToken);
                        await assignmentRepo.SaveChangesAsync(stoppingToken);

                        // Increment mailbox quota
                        mailbox.DailyQuotaUsed++;
                        mailbox.LastUsedAt = DateTime.UtcNow;
                        await mailboxRepo.UpdateAsync(mailbox, stoppingToken);
                        await mailboxRepo.SaveChangesAsync(stoppingToken);
                    }
                }

                _logger.LogInformation("Email dispatched: {Subject} → {To}, success={Success}, error={Error}, logId={LogId}",
                    msg.Subject, msg.ToEmail, outcome.Success, outcome.Error, log.LogId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch email to {To}", msg.ToEmail);
            }
        }

        _logger.LogInformation("EmailDispatcherService stopping.");
    }
}
