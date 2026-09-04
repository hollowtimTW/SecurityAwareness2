using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly AwarenessDbContext _db;
    public DepartmentRepository(AwarenessDbContext db) => _db = db;

    public Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id, ct);

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct = default)
        => await _db.Departments.AsNoTracking().OrderBy(d => d.Code).ToListAsync(ct);

    public async Task AddAsync(Department department, CancellationToken ct = default)
        => await _db.Departments.AddAsync(department, ct);

    public Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        _db.Departments.Update(department);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByCodeAsync(string code, CancellationToken ct = default)
        => _db.Departments.AnyAsync(d => d.Code == code, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
