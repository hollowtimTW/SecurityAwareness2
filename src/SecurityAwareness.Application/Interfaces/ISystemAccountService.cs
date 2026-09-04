using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Application.Interfaces;

public interface ISystemAccountService
{
    Task<SystemAccount?> AuthenticateAsync(string username, string password, CancellationToken ct = default);
    Task<SystemAccount?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
}
