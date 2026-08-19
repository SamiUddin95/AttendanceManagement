using System;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AttendanceManagement.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AttendanceDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<ILookupBoardService, LookupBoardService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IUserModuleService, UserModuleService>();

        return services;
    }
}
