using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class AssignGroup
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int GroupId { get; set; }

    // Information to be assigned
    [MaxLength(50)]
    public string? EmployeeType { get; set; } // Contract, Probation, Permanent

    public int? LocationId { get; set; }

    public int? DepartmentId { get; set; }

    // Incharge information
    public int? InchargeCategoryId { get; set; }

    public int? InchargeDesignationId { get; set; }

    public int? InchargeEmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public Group? Group { get; set; }
    public Location? Location { get; set; }
    public Department? Department { get; set; }
    public Category? InchargeCategory { get; set; }
    public Designation? InchargeDesignation { get; set; }
    public Employee? InchargeEmployee { get; set; }
}
