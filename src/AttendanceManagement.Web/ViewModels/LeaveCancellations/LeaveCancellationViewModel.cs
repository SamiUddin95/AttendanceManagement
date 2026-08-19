using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.LeaveCancellations;

public class LeaveCancellationViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Leave number is required")]
    [MaxLength(50)]
    public string LeaveNumber { get; set; } = string.Empty;

    // Leave details for display
    public string? EmployeeName { get; set; }
    public string? EmployeeNo { get; set; }
    public string? LeaveType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? TotalDays { get; set; }
    public string? Reason { get; set; }
    public string? Status { get; set; }
    public string? InchargeApprovalStatus { get; set; }
    public string? InchargeName { get; set; }

    // Cancellation
    [MaxLength(500)]
    public string? CancellationReason { get; set; }

    // Validation flags
    public bool CanCancel { get; set; }
    public string? CannotCancelReason { get; set; }
}

public class LeaveCancellationListViewModel
{
    public List<LeaveCancellationViewModel> CancelledLeaves { get; set; } = new();
}
