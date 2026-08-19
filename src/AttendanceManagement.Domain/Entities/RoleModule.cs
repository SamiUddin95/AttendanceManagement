using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class RoleModule
{
    [Key]
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int ModuleId { get; set; }

    public bool CanView { get; set; } = true;

    public bool CanCreate { get; set; } = false;

    public bool CanEdit { get; set; } = false;

    public bool CanDelete { get; set; } = false;

    public Role? Role { get; set; }

    public Module? Module { get; set; }
}
