using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Domain.Entities;

public class Category
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
