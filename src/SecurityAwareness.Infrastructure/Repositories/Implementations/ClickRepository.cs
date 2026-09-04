using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class ClickRepository : IClickRepository
{
    private readonly AwarenessDbContext _db;
    public ClickRepository(AwarenessDbContext db) => _db = db;

    public async Task AddAsync(PhishingLinkClick click, CancellationToken ct = default)
        => await _db.PhishingLinkClicks.AddAsync(click, ct);

    public async Task<IReadOnlyList<PhishingLinkClick>> GetByAssignmentAsync(int assignmentId, CancellationToken ct = default)
        => await _db.PhishingLinkClicks.AsNoTracking()
            .Where(c => c.AssignmentId == assignmentId)
            .OrderBy(c => c.ClickedAt)
            .ToListAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
