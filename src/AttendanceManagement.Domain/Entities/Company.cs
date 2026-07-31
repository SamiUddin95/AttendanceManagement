using System;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Company
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(25)]
    public string? PhoneNumber { get; set; }

    [MaxLength(25)]
    public string? FaxNumber { get; set; }

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

    public bool IsActive { get; set; } = true;

    [Required, MaxLength(100)]
    public string AdminUserId { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string AdminPasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Country? Country { get; set; }

    public City? City { get; set; }
}
