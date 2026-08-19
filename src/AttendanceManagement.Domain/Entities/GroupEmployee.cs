using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class GroupEmployee
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? RemovedAtUtc { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Group? Group { get; set; }
    public Employee? Employee { get; set; }
}
