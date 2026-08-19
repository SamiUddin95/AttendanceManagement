using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.LeaveHistory;

public class LeaveHistoryViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public List<LeaveHistoryItem> LeaveHistoryItems { get; set; } = new();
}

public class LeaveHistoryItem
{
    public int LeaveId { get; set; }
    public string LeaveNumber { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime DateApplied { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string? Reason { get; set; }
    public DateTime? InchargeApprovedAt { get; set; }
    public string? InchargeApprovalStatus { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
}
