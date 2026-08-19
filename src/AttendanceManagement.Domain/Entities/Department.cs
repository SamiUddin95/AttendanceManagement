using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Department
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public Company? Company { get; set; }

    public bool IsActive { get; set; } = true;
}
