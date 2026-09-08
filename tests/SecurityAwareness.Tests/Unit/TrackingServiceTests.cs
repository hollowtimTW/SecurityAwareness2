using FluentAssertions;
using Xunit;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace SecurityAwareness.Tests.Unit;

/// <summary>
/// Tests for TrackingService — record click, mark reported.
/// </summary>
public class TrackingServiceTests
{
    private static (AwarenessDbContext db, TrackingService svc, IAssignmentRepository assignmentRepo, ICampaignRepository campaignRepo) Build(int totalAssignments = 3)
    {
        var opts = new DbContextOptionsBuilder<AwarenessDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        var db = new AwarenessDbContext(opts);
        var assignments = new AssignmentRepository(db);
        var clicks = new ClickRepository(db);
        var audit = new AuditService(new AuditLogRepository(db), NullLogger<AuditService>.Instance);
        var svc = new TrackingService(assignments, clicks, audit, NullLogger<TrackingService>.Instance);
        return (db, svc, assignments, new CampaignRepository(db));
    }

    private static async Task<EmployeeCampaignAssignment> SeedAssignmentAsync(AwarenessDbContext db, IAssignmentRepository repo)
    {
        db.Departments.Add(new Department { Code = "T", Name = "T", CreatedAt = DateTime.Now });
        await db.SaveChangesAsync();

        db.Employees.Add(new Employee
        {
            EmployeeNo = "E1",
            Email = "e@test.com",
            DisplayName = "E",
            DepartmentId = 1,
            CreatedAt = DateTime.Now
        });
        db.PhishingCampaigns.Add(new PhishingCampaign
        {
            Code = "T1",
            Title = "T",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(1),
            CreatedBy = "test"
        });
        await db.SaveChangesAsync();

        var a = new EmployeeCampaignAssignment
        {
            CampaignId = 1,
            EmployeeId = 1,
            TrackingToken = "tok-1",
            Status = (byte)0,
            TokenUrl = "http://localhost/r/tok-1"
        };
        await repo.AddRangeAsync(new[] { a });
        await repo.SaveChangesAsync();
        return a;
    }

    [Fact]
    public async Task RecordClickAsync_ValidToken_IncrementsClickCount()
    {
        var (db, svc, repo, _) = Build();
        var a = await SeedAssignmentAsync(db, repo);

        await svc.RecordClickAsync("tok-1", "127.0.0.1", "Mozilla", 1);

        var reloaded = await repo.GetByTokenAsync("tok-1");
        reloaded!.ClickCount.Should().Be(1);
        reloaded.Status.Should().Be((byte)2); // Clicked
        reloaded.FirstClickedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RecordClickAsync_MultipleClicks_IncrementsEachTime()
    {
        var (db, svc, repo, _) = Build();
        var a = await SeedAssignmentAsync(db, repo);

        await svc.RecordClickAsync("tok-1", "1.1.1.1", "Mozilla", 1);
        await svc.RecordClickAsync("tok-1", "2.2.2.2", "Mozilla", 1);
        await svc.RecordClickAsync("tok-1", "3.3.3.3", "Mozilla", 1);

        var reloaded = await repo.GetByTokenAsync("tok-1");
        reloaded!.ClickCount.Should().Be(3);
    }

    [Fact]
    public async Task RecordClickAsync_UnknownToken_ReturnsNull()
    {
        var (_, svc, _, _) = Build();
        var result = await svc.RecordClickAsync("unknown-token", null, null, 1);
        result.Should().BeNull();
    }

    [Fact]
    public async Task MarkReportedAsync_ValidToken_SetsStatus()
    {
        var (db, svc, repo, _) = Build();
        var a = await SeedAssignmentAsync(db, repo);

        var result = await svc.MarkReportedAsync("tok-1");
        result.Should().BeTrue();

        var reloaded = await repo.GetByTokenAsync("tok-1");
        reloaded!.Status.Should().Be((byte)3); // Reported
        reloaded.ReportedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task MarkReportedAsync_UnknownToken_ReturnsFalse()
    {
        var (_, svc, _, _) = Build();
        var result = await svc.MarkReportedAsync("unknown");
        result.Should().BeFalse();
    }
}
