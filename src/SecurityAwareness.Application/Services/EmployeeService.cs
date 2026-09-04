using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repo;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(IEmployeeRepository repo, ILogger<EmployeeService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct);

    public async Task<Employee> CreateAsync(Employee employee, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(employee.EmployeeNo))
            throw new ArgumentException("EmployeeNo is required.", nameof(employee));
        if (string.IsNullOrWhiteSpace(employee.Email))
            throw new ArgumentException("Email is required.", nameof(employee));
        if (string.IsNullOrWhiteSpace(employee.DisplayName))
            throw new ArgumentException("DisplayName is required.", nameof(employee));

        if (await _repo.ExistsByEmailAsync(employee.Email, ct))
            throw new InvalidOperationException($"Email '{employee.Email}' already exists.");

        employee.CreatedAt = DateTime.Now;
        employee.IsActive = true;
        await _repo.AddAsync(employee, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Created Employee {EmployeeId} - {Email}", employee.EmployeeId, employee.Email);
        return employee;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(employee, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Updated Employee {EmployeeId}", employee.EmployeeId);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => _repo.ExistsByEmailAsync(email, ct);
}
