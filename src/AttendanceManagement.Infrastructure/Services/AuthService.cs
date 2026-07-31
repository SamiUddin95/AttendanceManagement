using System;
using System.Threading;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Auth;
using AttendanceManagement.Application.Contracts.Companies;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Infrastructure.Services.Mappers;
using AttendanceManagement.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

internal class AuthService : IAuthService
{
    private readonly AttendanceDbContext _dbContext;

    public AuthService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CompanyDto?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default)
    {
        var company = await FindCompanyAsync(request.CompanyIdentifier, cancellationToken);
        if (company is null || !company.IsActive)
        {
            return null;
        }

        var passwordValid = PasswordHasher.Verify(request.Password, company.AdminPasswordHash);
        if (!passwordValid || !string.Equals(company.AdminUserId, request.AdminUserId, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        await LoadNavigationAsync(company, cancellationToken);
        return company.ToDto();
    }

    private async Task<Company?> FindCompanyAsync(string identifier, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(identifier, out var companyId))
        {
            return await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        }

        return await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Domain == identifier || c.Name == identifier, cancellationToken);
    }

    private async Task LoadNavigationAsync(Company company, CancellationToken cancellationToken)
    {
        await _dbContext.Entry(company).Reference(c => c.Country).LoadAsync(cancellationToken);
        await _dbContext.Entry(company).Reference(c => c.City).LoadAsync(cancellationToken);
    }

}
