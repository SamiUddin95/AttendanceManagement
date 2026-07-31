using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Application.Contracts.Companies;

public class CompanyRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? FaxNumber { get; set; }

    [Range(1, 1000000)]
    public int? NumberOfEmployees { get; set; }

    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    public int CityId { get; set; }

    [MaxLength(150)]
    public string? Domain { get; set; }

    public bool IsActive { get; set; }

    [Required, MaxLength(100)]
    public string AdminUserId { get; set; } = string.Empty;

    [MinLength(6)]
    public string? AdminPassword { get; set; }
}
