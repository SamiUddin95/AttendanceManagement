using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Leave
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CompanyId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required]
    public int LeavePolicyId { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public Company? Company { get; set; }
    public Category? Category { get; set; }
    public LeavePolicy? LeavePolicy { get; set; }
    public List<LeaveDetail> LeaveDetails { get; set; } = new();
}
