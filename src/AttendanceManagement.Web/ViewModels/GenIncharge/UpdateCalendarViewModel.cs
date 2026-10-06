using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class UpdateCalendarViewModel
{
    // ── Filters ───────────────────────────────────────────────────────────────

    public int? SelectedEmployeeId { get; set; }
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<object>());

    public int SelectedMonth { get; set; } = DateTime.Now.Month;
    public int SelectedYear  { get; set; } = DateTime.Now.Year;

    public List<int> AvailableYears   { get; set; } = new();
    public List<CalendarMonthItem> AvailableMonths { get; set; } = new();

    // ── Employee header info ──────────────────────────────────────────────────

    public string? EmployeeName   { get; set; }
    public string? EmployeeNo     { get; set; }
    public string? Department     { get; set; }
    public string? Designation    { get; set; }

    // ── Calendar grid data ────────────────────────────────────────────────────

    public List<CalendarDayEntry> CalendarDays { get; set; } = new();

    // ── Display helpers ───────────────────────────────────────────────────────

    public string MonthYearDisplay =>
        new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");

    public bool HasCalendar => SelectedEmployeeId.HasValue && CalendarDays.Any();

    public static readonly List<string> DayStatusOptions = new()
    {
        "Working", "Off", "Sunday", "Gazetted", "Provincial", "Strike"
    };
}

public class CalendarDayEntry
{
    public DateTime Date      { get; set; }
    public string DayStatus   { get; set; } = "Working";
    public bool IsSunday      => Date.DayOfWeek == DayOfWeek.Sunday;
    public string DayLabel    => Date.ToString("ddd");
    public string DateKey     => Date.ToString("yyyy-MM-dd");
}

public class CalendarMonthItem
{
    public int Value    { get; set; }
    public string Name  { get; set; } = string.Empty;
}
