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
/// Tests for CampaignService state machine (Draft → Active → Completed).
/// Uses InMemory DB for isolation.
/// </summary>
public class CampaignServiceTests
{
    private static (AwarenessDbContext db, CampaignService svc, ISenderMailboxRepository mailboxRepo) Build()
    {
        var opts = new DbContextOptionsBuilder<AwarenessDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        var db = new AwarenessDbContext(opts);
        var campaigns = new CampaignRepository(db);
        var assignments = new AssignmentRepository(db);
        var employees = new EmployeeRepository(db);
        var mailboxRepo = new SenderMailboxRepository(db);
        var departmentRepo = new DepartmentRepository(db);
        var audit = new AuditService(new AuditLogRepository(db), NullLogger<AuditService>.Instance);
        var svc = new CampaignService(
            campaigns, assignments, employees, departmentRepo, mailboxRepo,
            new EmailQueueChannel(), new EmailMessageRenderer(),
            audit, NullLogger<CampaignService>.Instance);
        return (db, svc, mailboxRepo);
    }

    [Fact]
    public async Task CreateAsync_NewCampaign_StatusIsDraft()
    {
        var (db, svc, _) = Build();
        var c = new PhishingCampaign
        {
            Code = "TEST-1",
            Title = "Test",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(7)
        };
        var created = await svc.CreateAsync(c, "admin");
        created.Status.Should().Be((byte)0); // Draft
        created.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_Throws()
    {
        var (_, svc, _) = Build();
        var c = new PhishingCampaign
        {
            Code = "BAD",
            Title = "Bad",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddDays(-1)
        };
        Func<Task> act = () => svc.CreateAsync(c, "admin");
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateCode_Throws()
    {
        var (db, svc, _) = Build();
        await svc.CreateAsync(new PhishingCampaign
        {
            Code = "DUP", Title = "A", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)
        }, "admin");

        Func<Task> act = () => svc.CreateAsync(new PhishingCampaign
        {
            Code = "DUP", Title = "B", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(2)
        }, "admin");
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already exists*");
    }

    [Fact]
    public async Task StartAsync_DraftCampaign_BecomesActive()
    {
        var (_, svc, _) = Build();
        var c = await svc.CreateAsync(new PhishingCampaign
        {
            Code = "S1", Title = "Start Test",
            StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)
        }, "admin");
        await svc.StartAsync(c.CampaignId);
        var reloaded = await svc.GetByIdAsync(c.CampaignId);
        reloaded!.Status.Should().Be((byte)1); // Active
    }

    [Fact]
    public async Task StartAsync_AlreadyActive_Throws()
    {
        var (_, svc, _) = Build();
        var c = await svc.CreateAsync(new PhishingCampaign
        {
            Code = "S2", Title = "Start Twice",
            StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)
        }, "admin");
        await svc.StartAsync(c.CampaignId);

        Func<Task> act = () => svc.StartAsync(c.CampaignId);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EndAsync_ActiveCampaign_BecomesCompleted()
    {
        var (_, svc, _) = Build();
        var c = await svc.CreateAsync(new PhishingCampaign
        {
            Code = "E1", Title = "End",
            StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)
        }, "admin");
        await svc.StartAsync(c.CampaignId);
        await svc.EndAsync(c.CampaignId);

        var reloaded = await svc.GetByIdAsync(c.CampaignId);
        reloaded!.Status.Should().Be((byte)2); // Completed
    }
}
