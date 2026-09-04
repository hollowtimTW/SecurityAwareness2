using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Infrastructure.Entities;

namespace SecurityAwareness.Infrastructure.Persistence;

public class AwarenessDbContext : DbContext
{
    public AwarenessDbContext(DbContextOptions<AwarenessDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<PhishingCampaign> PhishingCampaigns => Set<PhishingCampaign>();
    public DbSet<CampaignSchedule> CampaignSchedules => Set<CampaignSchedule>();
    public DbSet<EmployeeCampaignAssignment> EmployeeCampaignAssignments => Set<EmployeeCampaignAssignment>();
    public DbSet<PhishingLinkClick> PhishingLinkClicks => Set<PhishingLinkClick>();
    public DbSet<SenderMailbox> SenderMailboxes => Set<SenderMailbox>();
    public DbSet<EmailDeliveryLog> EmailDeliveryLogs => Set<EmailDeliveryLog>();
    public DbSet<SystemAccount> SystemAccounts => Set<SystemAccount>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AwarenessDbContext).Assembly);
    }
}
