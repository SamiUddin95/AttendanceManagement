using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.Employees;

public class EmployeeViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CompanyId { get; set; }

    // Company Information
    [Required, MaxLength(50)]
    public string EmployeeType { get; set; } = "Permanent";

    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? DesignationId { get; set; }
    public int? ShiftId { get; set; }
    public int? LeaveId { get; set; }

    public DateTime? JoiningDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public DateTime? ResignationDate { get; set; }

    [MaxLength(10)]
    public string? EfficiencyRequired { get; set; }

    // Incharge
    public int? InchargeCategoryId { get; set; }
    public int? InchargeDesignationId { get; set; }
    public int? InchargeEmployeeId { get; set; }

    // Personal Information
    public string? ImageBase64 { get; set; }

    [Required, MaxLength(50)]
    public string EmployeeNo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? CardNo { get; set; }

    [MaxLength(150)]
    public string? FatherSpouseName { get; set; }

    [MaxLength(100)]
    public string? Channel { get; set; }

    [MaxLength(20)]
    public string? BloodGroup { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(20)]
    public string? Cell { get; set; }

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(255)]
    public string? Password { get; set; }

    [MaxLength(10)]
    public string? Gender { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(50)]
    public string? CNIC { get; set; }

    public bool OverTimeEntitled { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Display properties
    public string? CompanyName { get; set; }
    public string? DepartmentName { get; set; }
    public string? CategoryName { get; set; }
    public string? DesignationName { get; set; }
    public string? ShiftName { get; set; }
    public string? LeaveName { get; set; }
    public string? InchargeCategoryName { get; set; }
    public string? InchargeDesignationName { get; set; }
    public string? InchargeEmployeeName { get; set; }

    // Dropdown lists
    public List<SelectListItem>? Departments { get; set; }
    public List<SelectListItem>? Categories { get; set; }
    public List<SelectListItem>? Designations { get; set; }
    public List<SelectListItem>? Shifts { get; set; }
    public List<SelectListItem>? Leaves { get; set; }
    public List<SelectListItem>? InchargeCategories { get; set; }
    public List<SelectListItem>? InchargeDesignations { get; set; }
    public List<SelectListItem>? InchargeEmployees { get; set; }
    public List<SelectListItem>? EmployeeTypes { get; set; }
    public List<SelectListItem>? BloodGroups { get; set; }
    public List<SelectListItem>? Genders { get; set; }
    public List<SelectListItem>? EfficiencyOptions { get; set; }
}
