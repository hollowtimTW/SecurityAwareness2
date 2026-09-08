using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public sealed record EmailDeliveryQuery(
    int? CampaignId,
    byte? Status,
    int? MailboxId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page,
    int PageSize);

public sealed record EmailDeliveryPage(
    IReadOnlyList<EmailDeliveryLog> Items,
    int TotalCount,
    int Page,
    int PageSize);

public interface IEmailDeliveryLogRepository
{
    Task<EmailDeliveryPage> QueryAsync(EmailDeliveryQuery q, CancellationToken ct = default);
    Task<EmailDeliveryLog?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetCampaignIdsByMailboxAsync(int mailboxId, CancellationToken ct = default);
    Task AddAsync(EmailDeliveryLog log, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
