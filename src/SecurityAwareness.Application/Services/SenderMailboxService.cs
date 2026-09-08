using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class SenderMailboxService : ISenderMailboxService
{
    private readonly ISenderMailboxRepository _repo;
    private readonly ILogger<SenderMailboxService> _logger;

    public SenderMailboxService(ISenderMailboxRepository repo, ILogger<SenderMailboxService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SenderMailbox>> GetAllAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task<IReadOnlyList<SenderMailbox>> GetActiveAsync(CancellationToken ct = default)
        => await _repo.GetActiveAsync(ct);

    public async Task<SenderMailbox?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct);

    public async Task<SenderMailbox> CreateAsync(SenderMailbox mailbox, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mailbox.Email))
            throw new ArgumentException("Email is required.", nameof(mailbox));
        if (string.IsNullOrWhiteSpace(mailbox.DisplayName))
            throw new ArgumentException("DisplayName is required.", nameof(mailbox));

        if (await _repo.ExistsByEmailAsync(mailbox.Email, ct))
            throw new InvalidOperationException($"Email '{mailbox.Email}' already exists.");

        mailbox.CreatedAt = DateTime.Now;
        mailbox.IsActive = true;
        mailbox.DailyQuotaUsed = 0;
        await _repo.AddAsync(mailbox, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Created SenderMailbox {Id} - {Email}", mailbox.SenderMailboxId, mailbox.Email);
        return mailbox;
    }

    public async Task UpdateAsync(SenderMailbox mailbox, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(mailbox, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Updated SenderMailbox {Id}", mailbox.SenderMailboxId);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _repo.ExistsByEmailAsync(email, ct);

    public async Task SetActiveAsync(int id, bool isActive, CancellationToken ct = default)
    {
        var m = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Mailbox {id} not found.");
        m.IsActive = isActive;
        await _repo.UpdateAsync(m, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Mailbox {Id} IsActive set to {Active}", id, isActive);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var m = await _repo.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"Mailbox {id} not found.");
        await _repo.DeleteAsync(m, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Deleted Mailbox {Id} ({Email})", id, m.Email);
    }
}
