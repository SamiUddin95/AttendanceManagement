using AttendanceManagement.Web.ViewModels.InOutTimings;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class InchargeInOutTimingsViewModel
{
    public int? SelectedEmployeeId { get; set; }
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<object>());

    public string? SelectedEmployeeName { get; set; }
    public string? SelectedEmployeeNo { get; set; }
    public string? SelectedEmployeeDept { get; set; }
    public string? SelectedEmployeeDesignation { get; set; }

    [Required]
    public int SelectedMonth { get; set; } = DateTime.Now.Month;

    [Required]
    public int SelectedYear { get; set; } = DateTime.Now.Year;

    public bool IsPayrollWise { get; set; } = true;
    public bool IsMonthWise { get; set; } = false;

    public List<InOutTimingsItem> InOutTimingsItems { get; set; } = new();
    public List<int> AvailableYears { get; set; } = new();
    public List<MonthItem> AvailableMonths { get; set; } = new();

    public string ReportTypeDisplay => IsPayrollWise ? "Payroll Wise" : IsMonthWise ? "Month Wise" : "All";

    public string MonthYearDisplay => $"{GetMonthName(SelectedMonth)} {SelectedYear}";

    public int PresentCount => InOutTimingsItems.Count(i => i.Status == "Present");
    public int AbsentCount => InOutTimingsItems.Count(i => i.Status == "Absent");
    public int HalfDayCount => InOutTimingsItems.Count(i => i.Status.StartsWith("Half Day"));
    public TimeSpan TotalExtraHours => InOutTimingsItems
        .Where(i => i.ExtraHours.HasValue)
        .Aggregate(TimeSpan.Zero, (sum, i) => sum + i.ExtraHours!.Value);

    private static string GetMonthName(int month) => month switch
    {
        1 => "January",  2 => "February", 3 => "March",
        4 => "April",    5 => "May",       6 => "June",
        7 => "July",     8 => "August",    9 => "September",
        10 => "October", 11 => "November", 12 => "December",
        _ => "Unknown"
    };
}
