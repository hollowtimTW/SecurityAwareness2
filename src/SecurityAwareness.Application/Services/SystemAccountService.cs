using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly ISystemAccountRepository _repo;
    private readonly PasswordHasher _hasher;
    private readonly IAuditService _audit;
    private readonly ILogger<SystemAccountService> _logger;

    public SystemAccountService(
        ISystemAccountRepository repo,
        PasswordHasher hasher,
        IAuditService audit,
        ILogger<SystemAccountService> logger)
    {
        _repo = repo;
        _hasher = hasher;
        _audit = audit;
        _logger = logger;
    }

    public async Task<SystemAccount?> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var account = await _repo.GetByUsernameAsync(username, ct);
        if (account is null || !account.IsActive)
        {
            _logger.LogWarning("Auth failed: unknown or disabled user {User}", username);
            return null;
        }

        if (!_hasher.Verify(password, account.PasswordHash))
        {
            _logger.LogWarning("Auth failed: wrong password for {User}", username);
            return null;
        }

        account.LastLoginAt = DateTime.Now;
        await _repo.UpdateAsync(account, ct);
        await _repo.SaveChangesAsync(ct);
        await _audit.WriteAsync("Login", "SystemAccount", account.AccountId.ToString(), account.Username, null, ct);

        return account;
    }

    public async Task<SystemAccount?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => await _repo.GetByUsernameAsync(username, ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => _repo.ExistsByUsernameAsync(username, ct);
}
