using Microsoft.Extensions.DependencyInjection;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure.Repositories;

namespace SecurityAwareness.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAwarenessApplication(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICampaignService, CampaignService>();
        services.AddScoped<ITrackingService, TrackingService>();
        services.AddScoped<ISystemAccountService, SystemAccountService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ISenderMailboxService, SenderMailboxService>();
        services.AddScoped<ICampaignScheduleService, CampaignScheduleService>();

        // Password hasher is stateless and thread-safe
        services.AddSingleton<PasswordHasher>();

        // Email delivery — swap Fake → Graph later by changing this line
        services.AddScoped<IEmailDeliveryService, FakeEmailDeliveryService>();

        // Email queue channel + renderer are singletons (Channel is thread-safe; renderer is stateless)
        services.AddSingleton<IEmailQueueChannel, EmailQueueChannel>();
        services.AddSingleton<EmailMessageRenderer>();

        // Repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IAssignmentRepository, AssignmentRepository>();
        services.AddScoped<IClickRepository, ClickRepository>();
        services.AddScoped<ISenderMailboxRepository, SenderMailboxRepository>();
        services.AddScoped<ICampaignScheduleRepository, CampaignScheduleRepository>();
        services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IEmailDeliveryLogRepository, EmailDeliveryLogRepository>();

        return services;
    }
}
