namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class TeamLeaveRequestsViewModel
{
    public int InchargeId { get; set; }
    public string InchargeName { get; set; } = string.Empty;
    public List<TeamLeaveRequestItem> Requests { get; set; } = new();
    public int PendingCount => Requests.Count(r => r.InchargeApprovalStatus == "Pending" || r.InchargeApprovalStatus == null);
    public int ApprovedCount => Requests.Count(r => r.InchargeApprovalStatus == "Approved");
    public int RejectedCount => Requests.Count(r => r.InchargeApprovalStatus == "Rejected");
}

public class TeamLeaveRequestItem
{
    public int Id { get; set; }
    public string LeaveNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InchargeApprovalStatus { get; set; }
    public DateTime? InchargeApprovedAt { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
