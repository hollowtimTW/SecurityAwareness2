using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SecurityAwareness.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core CLI tools (e.g., `dotnet ef migrations add`)
/// to instantiate DbContext without running the full app startup.
/// </summary>
public class AwarenessDbContextFactory : IDesignTimeDbContextFactory<AwarenessDbContext>
{
    public AwarenessDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AwarenessDbContext>()
            .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=SecurityAwareness2Db;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;
        return new AwarenessDbContext(options);
    }
}
