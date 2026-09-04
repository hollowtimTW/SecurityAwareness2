using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SecurityAwareness.Infrastructure.Persistence;

namespace SecurityAwareness.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAwarenessInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AwarenessDbContext>(opts =>
            opts.UseSqlServer(connectionString, b => b.MigrationsAssembly("SecurityAwareness.Platform")));

        return services;
    }
}
