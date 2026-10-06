namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class TeamMembersViewModel
{
    public int InchargeId { get; set; }
    public string InchargeName { get; set; } = string.Empty;
    public string InchargeNo { get; set; } = string.Empty;
    public List<TeamMemberItem> Members { get; set; } = new();
}

public class TeamMemberItem
{
    public int Id { get; set; }
    public string EmployeeNo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public string? CategoryName { get; set; }
    public string? ShiftName { get; set; }
    public string? EmployeeType { get; set; }
    public DateTime? JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
