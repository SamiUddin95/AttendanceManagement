using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.AssignGroups;

public class AssignGroupViewModel
{
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required]
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    // Information to be assigned
    [MaxLength(50)]
    public string? EmployeeType { get; set; }

    public int? LocationId { get; set; }

    public int? DepartmentId { get; set; }

    // Incharge information
    public int? InchargeCategoryId { get; set; }

    public int? InchargeDesignationId { get; set; }

    public int? InchargeEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    // Dropdown options
    public List<GroupSelectItem> AvailableGroups { get; set; } = new();
    public List<LocationSelectItem> AvailableLocations { get; set; } = new();
    public List<DepartmentSelectItem> AvailableDepartments { get; set; } = new();
    public List<CategorySelectItem> AvailableInchargeCategories { get; set; } = new();
    public List<DesignationSelectItem> AvailableInchargeDesignations { get; set; } = new();
    public List<EmployeeSelectItem> AvailableInchargeEmployees { get; set; } = new();
}

public class GroupSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class LocationSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DepartmentSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CategorySelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class DesignationSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class EmployeeSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
}

public class AssignGroupListViewModel
{
    public List<AssignGroupViewModel> AssignGroups { get; set; } = new();
    public string? Search { get; set; }
}
