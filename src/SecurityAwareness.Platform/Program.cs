using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using SecurityAwareness.Application;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;
using SecurityAwareness.Platform.Filters;
using SecurityAwareness.Platform.HostedServices;
using SecurityAwareness.Platform.Services;
using System.Threading.RateLimiting;

// ===== Bootstrap Serilog (BEFORE WebApplicationBuilder) =====
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "SecurityAwareness2")
    .WriteTo.Console()
    .WriteTo.File(
        formatter: new CompactJsonFormatter(),
        path: "Logs/sa2-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Explicit webroot so static files (CSS/JS) work in dev and publish
    var webRoot = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
    if (!Directory.Exists(webRoot)) Directory.CreateDirectory(webRoot);
    builder.WebHost.UseWebRoot(webRoot);

    builder.Host.UseSerilog();

    // ===== DB =====
    var conn = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default missing.");
    builder.Services.AddAwarenessInfrastructure(conn);
    builder.Services.AddAwarenessApplication();

    // ===== Cookie Auth =====
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(opts =>
        {
            opts.LoginPath = "/Account/Login";
            opts.LogoutPath = "/Account/Logout";
            opts.ExpireTimeSpan = TimeSpan.FromHours(8);
            opts.SlidingExpiration = true;
            opts.Cookie.Name = ".SecurityAwareness.Auth";
            opts.Cookie.HttpOnly = true;
            opts.Cookie.SameSite = SameSiteMode.Lax;
            opts.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;
            opts.AccessDeniedPath = "/Account/Login";
            opts.ReturnUrlParameter = "returnUrl";
        });
    builder.Services.AddAuthorization(opts =>
    {
        opts.AddPolicy("Admin", p => p.RequireRole("Admin"));
        opts.AddPolicy("Operator", p => p.RequireRole("Admin", "Operator"));
        opts.AddPolicy("Auditor", p => p.RequireRole("Admin", "Auditor"));
    });

    // ===== Rate limiting (brute-force protection on login) =====
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        options.RejectionStatusCode = 429;
    });

    // ===== DataProtection keys persisted to disk (IIS multi-instance safe) =====
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(
            Path.Combine(builder.Environment.ContentRootPath, "..", "..", "..", "DataProtection-Keys")))
        .SetApplicationName("SecurityAwareness2");

    // ===== MVC + Razor =====
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<AuditActionFilter>();
    });
    builder.Services.AddScoped<AuditActionFilter>();

    // ===== App services =====
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, CurrentUser>();

    // ===== Health checks =====
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AwarenessDbContext>("database", tags: new[] { "ready" });

    // ===== Background services =====
    builder.Services.AddHostedService<EmailDispatcherService>();
    builder.Services.AddHostedService<CampaignSchedulerService>();
    builder.Services.AddHostedService<QuotaResetService>();
    builder.Services.AddHostedService<CampaignLifecycleService>();

    var app = builder.Build();

    // ===== Initialize DB (apply migrations + seed admin) =====
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AwarenessDbContext>();
        await DbInitializer.InitializeAsync(db, scope.ServiceProvider);
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "tracking",
        pattern: "r/{token}",
        defaults: new { controller = "Tracking", action = "Index" });

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    // Health endpoints
    app.MapHealthChecks("/health/live");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready") || check.Name == "database"
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
