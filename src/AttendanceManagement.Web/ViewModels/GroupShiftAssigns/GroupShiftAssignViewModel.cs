using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.GroupShiftAssigns;

public class GroupShiftAssignViewModel
{
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required]
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    // Assignment fields
    [Required]
    public DateTime AssignedShiftDate { get; set; }

    [Required]
    public int AssignedShiftId { get; set; }

    public string? AssignedShiftName { get; set; }

    // Employee list for the group
    public List<GroupEmployeeItem> GroupEmployees { get; set; } = new();

    // Selected employee IDs for assignment
    public List<int> SelectedEmployeeIds { get; set; } = new();

    // Dropdown options
    public List<GroupSelectItem> AvailableGroups { get; set; } = new();
    public List<ShiftSelectItem> AvailableShifts { get; set; } = new();
}

public class GroupEmployeeItem
{
    public int EmployeeId { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CurrentShiftId { get; set; }
    public string? CurrentShiftName { get; set; }
    public DateTime? CurrentAssignedShiftDate { get; set; }
    public bool IsSelected { get; set; }
}

public class GroupSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ShiftSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
