using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Companies;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Infrastructure.Services.Mappers;
using AttendanceManagement.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

internal class CompanyService : ICompanyService
{
    private readonly AttendanceDbContext _dbContext;

    public CompanyService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var companies = await _dbContext.Companies
            .Include(c => c.Country)
            .Include(c => c.City)
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return companies.Select(c => c.ToDto());
    }

    public async Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .Include(c => c.Country)
            .Include(c => c.City)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        return company is null ? null : company.ToDto();
    }

    public async Task<CompanyDto> CreateAsync(CompanyRequest request, CancellationToken cancellationToken = default)
    {
        var company = new Company
        {
            Name = request.Name,
            PhoneNumber = request.PhoneNumber,
            FaxNumber = request.FaxNumber,
            NumberOfEmployees = request.NumberOfEmployees,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            CountryId = request.CountryId,
            CityId = request.CityId,
            Domain = request.Domain,
            IsActive = request.IsActive,
            AdminUserId = request.AdminUserId,
            AdminPasswordHash = PasswordHasher.Hash(request.AdminPassword ?? throw new InvalidOperationException("Password required for new company."))
        };

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await LoadNavigationAsync(company, cancellationToken);
        return company.ToDto();
    }

    public async Task UpdateAsync(Guid id, CompanyRequest request, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                      ?? throw new KeyNotFoundException("Company not found.");

        company.Name = request.Name;
        company.PhoneNumber = request.PhoneNumber;
        company.FaxNumber = request.FaxNumber;
        company.NumberOfEmployees = request.NumberOfEmployees;
        company.AddressLine1 = request.AddressLine1;
        company.AddressLine2 = request.AddressLine2;
        company.CountryId = request.CountryId;
        company.CityId = request.CityId;
        company.Domain = request.Domain;
        company.IsActive = request.IsActive;
        company.AdminUserId = request.AdminUserId;
        if (!string.IsNullOrWhiteSpace(request.AdminPassword))
        {
            company.AdminPasswordHash = PasswordHasher.Hash(request.AdminPassword);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                      ?? throw new KeyNotFoundException("Company not found.");

        _dbContext.Companies.Remove(company);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> HasAnyAsync(CancellationToken cancellationToken = default)
        => _dbContext.Companies.AsNoTracking().AnyAsync(cancellationToken);

    private async Task LoadNavigationAsync(Company company, CancellationToken cancellationToken)
    {
        await _dbContext.Entry(company).Reference(c => c.Country).LoadAsync(cancellationToken);
        await _dbContext.Entry(company).Reference(c => c.City).LoadAsync(cancellationToken);
    }

}
