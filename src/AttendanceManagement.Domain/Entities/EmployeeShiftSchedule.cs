using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class EmployeeShiftSchedule
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int ShiftId { get; set; }

    // The schedule remains effective until the next schedule's EffectiveFrom date
    [Required]
    public DateTime EffectiveFrom { get; set; }

    public bool IsActive { get; set; } = true;

    public int? CreatedByEmployeeId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public int? UpdatedByEmployeeId { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }
    public Shift? Shift { get; set; }
    public Employee? CreatedByEmployee { get; set; }
    public Employee? UpdatedByEmployee { get; set; }
}
