using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CompanyId { get; set; }

    // Company Information
    [Required, MaxLength(50)]
    public string EmployeeType { get; set; } = "Permanent"; // Permanent, Contract, Part-Time, Temporary/Seasonal, Intern

    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? DesignationId { get; set; }
    public int? ShiftId { get; set; }
    public int? LeaveId { get; set; }

    public DateTime? AssignedShiftDate { get; set; }

    public DateTime? JoiningDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
    public DateTime? ResignationDate { get; set; }

    [MaxLength(10)]
    public string? EfficiencyRequired { get; set; } // e.g., "90%", "100%"

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
    public string? BloodGroup { get; set; } // A+, A-, B+, B-, AB+, AB-, O+, O-

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
    public string? Gender { get; set; } // Male, Female, Other

    public DateTime? DateOfBirth { get; set; }

    [MaxLength(50)]
    public string? CNIC { get; set; }

    public bool OverTimeEntitled { get; set; } = false;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Company? Company { get; set; }
    public Department? Department { get; set; }
    public Category? Category { get; set; }
    public Designation? Designation { get; set; }
    public Shift? Shift { get; set; }
    public Leave? Leave { get; set; }
    public Category? InchargeCategory { get; set; }
    public Designation? InchargeDesignation { get; set; }
    public Employee? InchargeEmployee { get; set; }
    public ICollection<GroupEmployee> GroupEmployees { get; set; } = new List<GroupEmployee>();
}
