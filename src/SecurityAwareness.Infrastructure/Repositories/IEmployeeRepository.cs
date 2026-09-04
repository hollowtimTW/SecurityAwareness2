using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Employee?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetActiveAsync(CancellationToken ct = default);
    Task AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
