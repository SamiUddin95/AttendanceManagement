using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class LeaveDetail
{
    [Key]
    public int Id { get; set; }

    public int LeaveId { get; set; }

    [Required, MaxLength(100)]
    public string LeaveType { get; set; } = string.Empty;

    public int NoOfDays { get; set; }

    public bool CarryForward { get; set; } = false;

    public Leave? Leave { get; set; }
}
