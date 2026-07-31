using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.Groups;

public class GroupViewModel
{
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Employment Type filter
    [MaxLength(50)]
    public string? EmploymentType { get; set; }

    // Gender filter
    [MaxLength(20)]
    public string? Gender { get; set; }

    // Date filters
    public DateTime? JoiningDateFrom { get; set; }
    public DateTime? JoiningDateTo { get; set; }
    public DateTime? PeriodEndDateFrom { get; set; }
    public DateTime? PeriodEndDateTo { get; set; }
    public DateTime? ResignationDateFrom { get; set; }
    public DateTime? ResignationDateTo { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }

    // Employee selection checkboxes
    public bool FilterByDepartment { get; set; } = false;
    public bool FilterByDesignation { get; set; } = false;
    public bool FilterByLocation { get; set; } = false;
    public bool FilterByIncharge { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Multi-select collections
    public List<int> SelectedDepartmentIds { get; set; } = new();
    public List<int> SelectedLocationIds { get; set; } = new();
    public List<int> SelectedDesignationIds { get; set; } = new();
    public List<int> SelectedInchargeIds { get; set; } = new();

    // Available options for dropdowns
    public List<DepartmentSelectItem> AvailableDepartments { get; set; } = new();
    public List<LocationSelectItem> AvailableLocations { get; set; } = new();
    public List<DesignationSelectItem> AvailableDesignations { get; set; } = new();
    public List<EmployeeSelectItem> AvailableIncharges { get; set; } = new();
}

public class DepartmentSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class LocationSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class DesignationSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class EmployeeSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class GroupListViewModel
{
    public List<GroupViewModel> Groups { get; set; } = new();
    public string? Search { get; set; }
}
