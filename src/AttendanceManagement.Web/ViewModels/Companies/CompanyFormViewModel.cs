using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.Companies;

public class CompanyFormViewModel
{
    public Guid? Id { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Company Name")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Phone No")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Fax No")]
    public string? FaxNumber { get; set; }

    [Display(Name = "No of Employees")]
    public int? NumberOfEmployees { get; set; }

    [Display(Name = "Address 1")]
    public string? AddressLine1 { get; set; }

    [Display(Name = "Address 2")]
    public string? AddressLine2 { get; set; }

    [Required]
    public int CountryId { get; set; }

    [Required]
    public int CityId { get; set; }

    [Display(Name = "Domain")]
    public string? Domain { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Required]
    [Display(Name = "Admin User ID")]
    public string AdminUserId { get; set; } = string.Empty;

    [Display(Name = "Admin Password")]
    [DataType(DataType.Password)]
    public string? AdminPassword { get; set; }

    public IEnumerable<SelectListItem> Countries { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Cities { get; set; } = new List<SelectListItem>();
}
