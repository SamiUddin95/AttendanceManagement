using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.Leaves;

public class LeaveDetailViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string LeaveType { get; set; } = string.Empty;

    public int NoOfDays { get; set; }

    public bool CarryForward { get; set; } = false;
}
