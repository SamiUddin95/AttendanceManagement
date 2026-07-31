using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Group
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Employment Type filter
    [MaxLength(50)]
    public string? EmploymentType { get; set; } // Contract, Probation, Permanent

    // Gender filter
    [MaxLength(20)]
    public string? Gender { get; set; } // Male, Female

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

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public ICollection<GroupDepartment> GroupDepartments { get; set; } = new List<GroupDepartment>();
    public ICollection<GroupLocation> GroupLocations { get; set; } = new List<GroupLocation>();
    public ICollection<GroupDesignation> GroupDesignations { get; set; } = new List<GroupDesignation>();
    public ICollection<GroupIncharge> GroupIncharges { get; set; } = new List<GroupIncharge>();
}

// Junction table for Group-Department (many-to-many)
public class GroupDepartment
{
    public int GroupId { get; set; }
    public int DepartmentId { get; set; }

    public Group? Group { get; set; }
    public Department? Department { get; set; }
}

// Junction table for Group-Location (many-to-many)
public class GroupLocation
{
    public int GroupId { get; set; }
    public int LocationId { get; set; }

    public Group? Group { get; set; }
    public Location? Location { get; set; }
}

// Junction table for Group-Designation (many-to-many)
public class GroupDesignation
{
    public int GroupId { get; set; }
    public int DesignationId { get; set; }

    public Group? Group { get; set; }
    public Designation? Designation { get; set; }
}

// Junction table for Group-Incharge (many-to-many with Employee)
public class GroupIncharge
{
    public int GroupId { get; set; }
    public int EmployeeId { get; set; }

    public Group? Group { get; set; }
    public Employee? Employee { get; set; }
}
