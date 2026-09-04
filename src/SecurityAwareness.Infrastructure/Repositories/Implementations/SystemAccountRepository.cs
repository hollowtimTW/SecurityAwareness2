using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class SystemAccountRepository : ISystemAccountRepository
{
    private readonly AwarenessDbContext _db;
    public SystemAccountRepository(AwarenessDbContext db) => _db = db;

    public Task<SystemAccount?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _db.SystemAccounts.FirstOrDefaultAsync(a => a.Username == username, ct);

    public Task<SystemAccount?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.SystemAccounts.FirstOrDefaultAsync(a => a.AccountId == id, ct);

    public Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default)
        => _db.SystemAccounts.AnyAsync(a => a.Username == username, ct);

    public async Task AddAsync(SystemAccount account, CancellationToken ct = default)
        => await _db.SystemAccounts.AddAsync(account, ct);

    public Task UpdateAsync(SystemAccount account, CancellationToken ct = default)
    {
        _db.SystemAccounts.Update(account);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
