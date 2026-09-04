using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface ISenderMailboxRepository
{
    Task<SenderMailbox?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SenderMailbox>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SenderMailbox>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(SenderMailbox mailbox, CancellationToken ct = default);
    Task UpdateAsync(SenderMailbox mailbox, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task IncrementQuotaAsync(int id, CancellationToken ct = default);
    Task ResetAllDailyQuotasAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
