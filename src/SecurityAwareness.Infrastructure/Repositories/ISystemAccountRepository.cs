using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface ISystemAccountRepository
{
    Task<SystemAccount?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<SystemAccount?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken ct = default);
    Task AddAsync(SystemAccount account, CancellationToken ct = default);
    Task UpdateAsync(SystemAccount account, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
