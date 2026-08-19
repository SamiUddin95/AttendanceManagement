using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class RoleAssignment
{
    [Key]
    public int Id { get; set; }

    public int RoleId { get; set; }

    public Guid CompanyId { get; set; }

    public string AssignmentType { get; set; } = "Group"; // "Group" or "Individual"

    public int? GroupId { get; set; }

    public int? DepartmentId { get; set; }

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    public Role? Role { get; set; }

    public Company? Company { get; set; }

    public Group? Group { get; set; }

    public Department? Department { get; set; }

    public Employee? Employee { get; set; }
}
