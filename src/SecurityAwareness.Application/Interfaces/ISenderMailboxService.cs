using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface ISenderMailboxService
{
    Task<IReadOnlyList<SenderMailbox>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SenderMailbox>> GetActiveAsync(CancellationToken ct = default);
    Task<SenderMailbox?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<SenderMailbox> CreateAsync(SenderMailbox mailbox, CancellationToken ct = default);
    Task UpdateAsync(SenderMailbox mailbox, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
