namespace AttendanceManagement.Application.Contracts.Companies;

public record CompanyDto(
    Guid Id,
    string Name,
    string? PhoneNumber,
    string? FaxNumber,
    int? NumberOfEmployees,
    string? AddressLine1,
    string? AddressLine2,
    int CountryId,
    string CountryName,
    int CityId,
    string CityName,
    string? Domain,
    bool IsActive,
    string AdminUserId,
    DateTime CreatedAtUtc);
