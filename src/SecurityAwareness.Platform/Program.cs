using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityAwareness.Application;
using SecurityAwareness.Application.Interfaces;
using SecurityAwareness.Application.Services;
using SecurityAwareness.Infrastructure;
using SecurityAwareness.Infrastructure.Persistence;
using SecurityAwareness.Infrastructure.Repositories;
using SecurityAwareness.Platform.Filters;
using SecurityAwareness.Platform.HostedServices;
using SecurityAwareness.Platform.Services;

var builder = WebApplication.CreateBuilder(args);

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
    });
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("Admin", p => p.RequireRole("Admin"));
    opts.AddPolicy("Operator", p => p.RequireRole("Admin", "Operator"));
    opts.AddPolicy("Auditor", p => p.RequireRole("Admin", "Auditor"));
});

// ===== MVC + Razor =====
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>();
});
builder.Services.AddScoped<AuditActionFilter>();

// ===== App services =====
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// ===== Background services =====
builder.Services.AddHostedService<EmailDispatcherService>();
builder.Services.AddHostedService<CampaignSchedulerService>();

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

app.Run();

public partial class Program { }
