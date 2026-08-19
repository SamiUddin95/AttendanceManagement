using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class MarkDay
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int GroupId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    public int TotalDays { get; set; }

    // Status checkboxes
    public bool IsOn { get; set; } = false;
    public bool IsOff { get; set; } = false;
    public bool IsGazettedHoliday { get; set; } = false;
    public bool IsProvincialHoliday { get; set; } = false;
    public bool IsStrike { get; set; } = false;
    public bool IsEid { get; set; } = false;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation properties
    public Company? Company { get; set; }
    public Group? Group { get; set; }
}
