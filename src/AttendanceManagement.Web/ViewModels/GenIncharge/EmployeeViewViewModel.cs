using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class EmployeeViewViewModel
{
    public int? SelectedEmployeeId { get; set; }
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<object>());
    public EmployeeProfileDetail? Profile { get; set; }
    public List<EmployeeLeaveBalance> LeaveBalances { get; set; } = new();
    public List<EmployeeLeaveHistoryItem> LeaveHistory { get; set; } = new();
    public int TotalEntitledDays => LeaveBalances.Sum(b => b.EntitledDays);
    public int TotalUsedDays => LeaveBalances.Sum(b => b.UsedDays);
    public int TotalRemainingDays => LeaveBalances.Sum(b => b.RemainingDays);
}

public class EmployeeProfileDetail
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
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Cell { get; set; }
    public string? Address { get; set; }
    public string? CNIC { get; set; }
    public bool IsActive { get; set; }
    public bool OverTimeEntitled { get; set; }
    public string? LeaveName { get; set; }
}

public class EmployeeLeaveBalance
{
    public string LeaveType { get; set; } = string.Empty;
    public int EntitledDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public bool CarryForward { get; set; }
    public double UsagePercentage => EntitledDays > 0 ? (UsedDays * 100.0 / EntitledDays) : 0;
}

public class EmployeeLeaveHistoryItem
{
    public int Id { get; set; }
    public string LeaveNumber { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InchargeApprovalStatus { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
