using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.Employees;

public class AssignTeamViewModel
{
    public int? InchargeId { get; set; }
    public List<int> EmployeeIds { get; set; } = [];
    public bool AllowReassignment { get; set; }
    [BindNever] public List<SelectListItem> Incharges { get; set; } = [];
    [BindNever] public List<TeamCandidate> Candidates { get; set; } = [];
    [BindNever] public string? ReportsTo { get; set; }
}

public record TeamCandidate(int Id, string Name, string EmployeeNo, string? Incharge, bool IsMember, bool IsActive);
