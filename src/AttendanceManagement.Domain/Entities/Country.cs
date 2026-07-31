using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Domain.Entities;

public class Country
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<City> Cities { get; set; } = new List<City>();

    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
