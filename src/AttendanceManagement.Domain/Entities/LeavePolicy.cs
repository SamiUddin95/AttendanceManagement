using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class LeavePolicy
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Active";

    public Guid CompanyId { get; set; }

    public Company? Company { get; set; }
}
