using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AttendanceManagement.Domain.Entities;

public class City
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [ForeignKey(nameof(Country))]
    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
