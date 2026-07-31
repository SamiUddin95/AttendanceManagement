namespace AttendanceManagement.Domain.Entities;

public class Shift
{
    public int Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Company Company { get; set; } = null!;
    public ICollection<ShiftDay> ShiftDays { get; set; } = new List<ShiftDay>();
}
