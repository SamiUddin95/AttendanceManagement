namespace AttendanceManagement.Web.ViewModels.LeaveFlexisChart;

public class LeaveFlexisChartViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public List<LeaveBalanceItem> LeaveBalances { get; set; } = new();
    public int TotalEntitledDays { get; set; }
    public int TotalUsedDays { get; set; }
    public int TotalRemainingDays { get; set; }
}

public class LeaveBalanceItem
{
    public string LeaveType { get; set; } = string.Empty;
    public int EntitledDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public bool CarryForward { get; set; }
    public double UsagePercentage => EntitledDays > 0 ? (double)UsedDays / EntitledDays * 100 : 0;
}
