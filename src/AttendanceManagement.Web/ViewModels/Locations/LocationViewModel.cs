using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.Locations;

public class LocationViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [MaxLength(25)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}

public class LocationListViewModel
{
    public List<LocationViewModel> Locations { get; set; } = new();
    public string? Search { get; set; }
}
