using FluentAssertions;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace SecurityAwareness.Tests.Unit;

/// <summary>
/// Tests for EmployeeService — validation rules + uniqueness.
/// </summary>
public class EmployeeServiceTests
{
    private static (AwarenessDbContext db, EmployeeService svc, DepartmentService deptSvc) Build()
    {
        var opts = new DbContextOptionsBuilder<AwarenessDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        var db = new AwarenessDbContext(opts);
        var employeeSvc = new EmployeeService(
            new EmployeeRepository(db),
            NullLogger<EmployeeService>.Instance);
        var deptSvc = new DepartmentService(
            new DepartmentRepository(db),
            NullLogger<DepartmentService>.Instance);
        return (db, employeeSvc, deptSvc);
    }

    [Fact]
    public async Task CreateAsync_ValidEmployee_Persists()
    {
        var (db, svc, deptSvc) = Build();
        await deptSvc.CreateAsync(new Department { Code = "T", Name = "Test" });

        var emp = new Employee
        {
            EmployeeNo = "E001",
            Email = "e1@test.com",
            DisplayName = "E",
            DepartmentId = 1
        };
        var created = await svc.CreateAsync(emp);
        created.EmployeeId.Should().BeGreaterThan(0);
        created.CreatedAt.Should().BeAfter(DateTime.MinValue);
    }

    [Fact]
    public async Task CreateAsync_MissingEmail_Throws()
    {
        var (_, svc, deptSvc) = Build();
        await deptSvc.CreateAsync(new Department { Code = "T", Name = "Test" });
        var emp = new Employee { EmployeeNo = "E1", DisplayName = "X", DepartmentId = 1 };

        Func<Task> act = () => svc.CreateAsync(emp);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Email*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_Throws()
    {
        var (_, svc, deptSvc) = Build();
        await deptSvc.CreateAsync(new Department { Code = "T", Name = "Test" });
        await svc.CreateAsync(new Employee
        {
            EmployeeNo = "E1", Email = "dup@test.com", DisplayName = "X", DepartmentId = 1
        });

        Func<Task> act = () => svc.CreateAsync(new Employee
        {
            EmployeeNo = "E2", Email = "dup@test.com", DisplayName = "Y", DepartmentId = 1
        });
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task CreateAsync_DefaultIsActive()
    {
        var (_, svc, deptSvc) = Build();
        await deptSvc.CreateAsync(new Department { Code = "T", Name = "Test" });
        var emp = await svc.CreateAsync(new Employee
        {
            EmployeeNo = "E1", Email = "x@t.com", DisplayName = "X", DepartmentId = 1
        });
        emp.IsActive.Should().BeTrue();
    }
}
