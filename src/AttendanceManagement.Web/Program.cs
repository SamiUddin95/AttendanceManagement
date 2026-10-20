using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using System;
using AttendanceManagement.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AttendanceManagement.Web.Security.ModuleAccessFilter>();
    options.Filters.Add<AttendanceManagement.Web.Security.DatabaseConflictFilter>();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddInfrastructure(builder.Configuration);

// Sessions from a demo database must never authenticate against a different database.
var databaseConnection = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
var databaseIdentity = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    $"{databaseConnection.DataSource}|{databaseConnection.InitialCatalog}".ToUpperInvariant())));
builder.Services.AddDataProtection().SetApplicationName($"AttendanceManagement:{databaseIdentity}");

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = $"AttendanceManagement.Auth.{databaseIdentity[..16]}";
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

var app = builder.Build();

if (args.Contains("--seed-demo"))
{
    using var scope = app.Services.CreateScope();
    await AttendanceManagement.Web.Testing.DemoData.SeedAsync(scope.ServiceProvider.GetRequiredService<AttendanceManagement.Infrastructure.Data.AttendanceDbContext>(), app.Environment, app.Configuration);
    return;
}

if (args.Contains("--seed-sample-data"))
{
    if (!Guid.TryParse(app.Configuration["Samples:CompanyId"], out var companyId))
        throw new InvalidOperationException("Specify Samples__CompanyId for the existing company.");
    using var scope = app.Services.CreateScope();
    await AttendanceManagement.Web.Testing.DemoData.SeedAsync(
        scope.ServiceProvider.GetRequiredService<AttendanceManagement.Infrastructure.Data.AttendanceDbContext>(),
        app.Environment, app.Configuration, companyId);
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
