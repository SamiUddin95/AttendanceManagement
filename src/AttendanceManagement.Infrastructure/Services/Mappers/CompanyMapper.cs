using AttendanceManagement.Application.Contracts.Companies;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Infrastructure.Services.Mappers;

internal static class CompanyMapper
{
    public static CompanyDto ToDto(this Company entity)
        => new(
            entity.Id,
            entity.Name,
            entity.PhoneNumber,
            entity.FaxNumber,
            entity.NumberOfEmployees,
            entity.AddressLine1,
            entity.AddressLine2,
            entity.CountryId,
            entity.Country?.Name ?? string.Empty,
            entity.CityId,
            entity.City?.Name ?? string.Empty,
            entity.Domain,
            entity.IsActive,
            entity.AdminUserId,
            entity.CreatedAtUtc);
}
