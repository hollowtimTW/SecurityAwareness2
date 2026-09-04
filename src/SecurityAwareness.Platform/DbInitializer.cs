using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Common.Enums;
using SecurityAwareness.Infrastructure.Entities;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure.Persistence;

/// <summary>
/// One-time DB seeding: default admin user + 4 departments.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(AwarenessDbContext db, IServiceProvider services)
    {
        await db.Database.MigrateAsync();

        // Seed departments
        if (!await db.Departments.AnyAsync())
        {
            db.Departments.AddRange(
                new Department { Code = "SYS", Name = "系統部", CreatedAt = DateTime.Now },
                new Department { Code = "PROJ", Name = "專案部", CreatedAt = DateTime.Now },
                new Department { Code = "MGMT", Name = "管理部", CreatedAt = DateTime.Now },
                new Department { Code = "CEO", Name = "總經理室", CreatedAt = DateTime.Now }
            );
            await db.SaveChangesAsync();
        }

        // Seed admin account
        if (!await db.SystemAccounts.AnyAsync())
        {
            var hasher = services.GetRequiredService<PasswordHasher>();
            db.SystemAccounts.Add(new SystemAccount
            {
                Username = "admin",
                PasswordHash = hasher.Hash("Admin@123"),
                DisplayName = "系統管理員",
                Role = (byte)SystemRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.Now
            });
            await db.SaveChangesAsync();
        }
    }
}
