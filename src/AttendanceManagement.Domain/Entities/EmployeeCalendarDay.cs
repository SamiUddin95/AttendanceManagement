using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class EmployeeCalendarDay
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required, MaxLength(30)]
    public string DayStatus { get; set; } = "Working";
    // Values: Working | Off | Sunday | Gazetted | Provincial | Strike

    public int? UpdatedByEmployeeId { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }
    public Employee? UpdatedByEmployee { get; set; }
}
