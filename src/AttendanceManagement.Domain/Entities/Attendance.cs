using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Attendance
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public DateTime PunchDateTime { get; set; }

    [Required, MaxLength(10)]
    public string PunchType { get; set; } = string.Empty; // In, Out

    [MaxLength(50)]
    public string? DeviceId { get; set; }

    [MaxLength(100)]
    public string? Location { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Company? Company { get; set; }
    public Employee? Employee { get; set; }
}
