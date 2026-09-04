using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly AwarenessDbContext _db;
    public EmployeeRepository(AwarenessDbContext db) => _db = db;

    public Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        => _db.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.EmployeeId == id, ct);

    public Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Employees.FirstOrDefaultAsync(e => e.Email == email, ct);

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
        => await _db.Employees.AsNoTracking().Include(e => e.Department).OrderBy(e => e.EmployeeNo).ToListAsync(ct);

    public async Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken ct = default)
        => await _db.Employees.AsNoTracking().Where(e => e.IsActive).Include(e => e.Department).OrderBy(e => e.EmployeeNo).ToListAsync(ct);

    public async Task AddAsync(Employee employee, CancellationToken ct = default)
        => await _db.Employees.AddAsync(employee, ct);

    public Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        _db.Employees.Update(employee);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => _db.Employees.AnyAsync(e => e.Email == email, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
