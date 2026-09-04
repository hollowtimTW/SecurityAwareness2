using Microsoft.Extensions.Logging;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repo;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(IDepartmentRepository repo, ILogger<DepartmentService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct = default)
        => await _repo.GetAllAsync(ct);

    public async Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _repo.GetByIdAsync(id, ct);

    public async Task<Department> CreateAsync(Department department, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(department.Code))
            throw new ArgumentException("Code is required.", nameof(department));
        if (string.IsNullOrWhiteSpace(department.Name))
            throw new ArgumentException("Name is required.", nameof(department));

        if (await _repo.ExistsByCodeAsync(department.Code, ct))
            throw new InvalidOperationException($"Code '{department.Code}' already exists.");

        department.CreatedAt = DateTime.Now;
        department.IsActive = true;
        await _repo.AddAsync(department, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Created Department {Id} - {Code}", department.DepartmentId, department.Code);
        return department;
    }

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        await _repo.UpdateAsync(department, ct);
        await _repo.SaveChangesAsync(ct);
        _logger.LogInformation("Updated Department {Id}", department.DepartmentId);
    }
}
