namespace AttendanceManagement.Web.ViewModels.LeavePolicy;

public class EmployeeLeavePolicyViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string LeavePolicyName { get; set; } = string.Empty;
    public string LeavePolicyStatus { get; set; } = string.Empty;
    public string LeaveName { get; set; } = string.Empty;
    public string LeaveDescription { get; set; } = string.Empty;
    public List<LeaveDetailViewModel> LeaveDetails { get; set; } = new();
}

public class LeaveDetailViewModel
{
    public string LeaveType { get; set; } = string.Empty;
    public int NoOfDays { get; set; }
    public bool CarryForward { get; set; }
}
