using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.ApplyLeave;

public class ApplyLeaveViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public List<LeaveTypeOption> LeaveTypes { get; set; } = new();
    public List<LeaveBalanceItem> LeaveBalances { get; set; } = new();
    public SelectList LeaveTypeOptions { get; set; } = new SelectList(new List<SelectListItem>());

    [Required]
    [Display(Name = "Leave Type")]
    public string SelectedLeaveType { get; set; } = string.Empty;

    [Required]
    [Display(Name = "From Date")]
    [DataType(DataType.Date)]
    public DateTime FromDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    [Display(Name = "To Date")]
    [DataType(DataType.Date)]
    public DateTime ToDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    [MaxLength(500)]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;

    public int TotalDays => (ToDate - FromDate).Days + 1;
}

public class LeaveTypeOption
{
    public string LeaveType { get; set; } = string.Empty;
    public int AvailableDays { get; set; }
    public bool CarryForward { get; set; }
}

public class LeaveBalanceItem
{
    public string LeaveType { get; set; } = string.Empty;
    public int EntitledDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }
    public bool CarryForward { get; set; }
}
