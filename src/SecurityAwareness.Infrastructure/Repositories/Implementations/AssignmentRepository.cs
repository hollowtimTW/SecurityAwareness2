using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AwarenessDbContext _db;
    public AssignmentRepository(AwarenessDbContext db) => _db = db;

    public Task<EmployeeCampaignAssignment?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.EmployeeCampaignAssignments.Include(a => a.Employee).Include(a => a.Campaign)
            .FirstOrDefaultAsync(a => a.AssignmentId == id, ct);

    public Task<EmployeeCampaignAssignment?> GetByTokenAsync(string token, CancellationToken ct = default)
        => _db.EmployeeCampaignAssignments.Include(a => a.Employee).Include(a => a.Campaign)
            .FirstOrDefaultAsync(a => a.TrackingToken == token, ct);

    public async Task<IReadOnlyList<EmployeeCampaignAssignment>> GetByCampaignAsync(int campaignId, CancellationToken ct = default)
        => await _db.EmployeeCampaignAssignments.AsNoTracking()
            .Where(a => a.CampaignId == campaignId)
            .Include(a => a.Employee)
            .ToListAsync(ct);

    public async Task AddRangeAsync(IEnumerable<EmployeeCampaignAssignment> assignments, CancellationToken ct = default)
        => await _db.EmployeeCampaignAssignments.AddRangeAsync(assignments, ct);

    public Task UpdateAsync(EmployeeCampaignAssignment assignment, CancellationToken ct = default)
    {
        _db.EmployeeCampaignAssignments.Update(assignment);
        return Task.CompletedTask;
    }

    public async Task IncrementClickCountAsync(int assignmentId, CancellationToken ct = default)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE EmployeeCampaignAssignments
               SET ClickCount = ClickCount + 1,
                   FirstClickedAt = ISNULL(FirstClickedAt, GETDATE()),
                   Status = CASE WHEN Status IN (0, 1) THEN 2 ELSE Status END
               WHERE AssignmentId = {assignmentId}", ct);
    }

    public Task<int> CountByCampaignAsync(int campaignId, CancellationToken ct = default)
        => _db.EmployeeCampaignAssignments.CountAsync(a => a.CampaignId == campaignId, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
