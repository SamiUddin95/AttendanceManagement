using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Role
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<RoleModule> RoleModules { get; set; } = new List<RoleModule>();

    public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
}
