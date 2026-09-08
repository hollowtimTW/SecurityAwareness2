using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class SenderMailboxRepository : ISenderMailboxRepository
{
    private readonly AwarenessDbContext _db;
    public SenderMailboxRepository(AwarenessDbContext db) => _db = db;

    public Task<SenderMailbox?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.SenderMailboxes.FirstOrDefaultAsync(m => m.SenderMailboxId == id, ct);

    public async Task<IReadOnlyList<SenderMailbox>> GetAllAsync(CancellationToken ct = default)
        => await _db.SenderMailboxes.AsNoTracking().OrderBy(m => m.Email).ToListAsync(ct);

    public async Task<IReadOnlyList<SenderMailbox>> GetActiveAsync(CancellationToken ct = default)
        => await _db.SenderMailboxes.AsNoTracking()
            .Where(m => m.IsActive)
            .OrderBy(m => m.SenderMailboxId)
            .ToListAsync(ct);

    public async Task AddAsync(SenderMailbox mailbox, CancellationToken ct = default)
        => await _db.SenderMailboxes.AddAsync(mailbox, ct);

    public Task UpdateAsync(SenderMailbox mailbox, CancellationToken ct = default)
    {
        _db.SenderMailboxes.Update(mailbox);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(SenderMailbox mailbox, CancellationToken ct = default)
    {
        _db.SenderMailboxes.Remove(mailbox);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _db.SenderMailboxes.AnyAsync(m => m.Email == email, ct);

    public async Task IncrementQuotaAsync(int id, CancellationToken ct = default)
        => await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE SenderMailboxes SET DailyQuotaUsed = DailyQuotaUsed + 1 WHERE SenderMailboxId = {id}", ct);

    public async Task ResetAllDailyQuotasAsync(CancellationToken ct = default)
        => await _db.Database.ExecuteSqlRawAsync("UPDATE SenderMailboxes SET DailyQuotaUsed = 0", ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
