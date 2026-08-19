using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.InOutTimings;

public class InOutTimingsViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;

    [Required]
    public int SelectedMonth { get; set; }

    [Required]
    public int SelectedYear { get; set; }

    public bool IsPayrollWise { get; set; } = true;
    public bool IsMonthWise { get; set; } = false;

    public List<InOutTimingsItem> InOutTimingsItems { get; set; } = new();
    public string ReportTypeDisplay => IsPayrollWise ? "Payroll Wise" : "Month Wise";
    public string MonthYearDisplay => $"{GetMonthName(SelectedMonth)} {SelectedYear}";

    public List<int> AvailableYears { get; set; } = new();
    public List<MonthItem> AvailableMonths { get; set; } = new();

    private static string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => "Unknown"
        };
    }
}

public class InOutTimingsItem
{
    public int SrNo { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? TimeIn { get; set; }
    public TimeSpan? TimeOut { get; set; }
    public TimeSpan? ExtraHours { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Absent, Half Day, etc.
}

public class MonthItem
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
}
