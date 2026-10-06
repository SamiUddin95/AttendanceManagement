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
    public string? ShiftName      { get; set; }

    // ── Calendar grid data ────────────────────────────────────────────────────

    public List<CalendarDayEntry> CalendarDays { get; set; } = new();

    // ── Display helpers ───────────────────────────────────────────────────────

    public string MonthYearDisplay =>
        new DateTime(SelectedYear, SelectedMonth, 1).ToString("MMMM yyyy");

    public bool HasCalendar => SelectedEmployeeId.HasValue && CalendarDays.Any();

    public int OverriddenCount => CalendarDays.Count(d => d.IsOverridden);

    public static readonly List<string> DayStatusOptions = new()
    {
        "Working", "Off", "Sunday", "Gazetted", "Provincial", "Strike"
    };
}

public class CalendarDayEntry
{
    public DateTime Date          { get; set; }
    public string DayStatus       { get; set; } = "Working";
    public string DefaultStatus   { get; set; } = "Working";
    public string? DefaultSource  { get; set; }   // e.g. "General Shift" or "Gazetted Holiday"
    public bool IsSunday          => Date.DayOfWeek == DayOfWeek.Sunday;
    public bool IsOverridden      => !string.Equals(DayStatus, DefaultStatus, StringComparison.OrdinalIgnoreCase);
    public string DayLabel        => Date.ToString("ddd");
    public string DateKey         => Date.ToString("yyyy-MM-dd");

    // "Sunday" is only offered where it is meaningful (actual Sundays or shift weekly-off days)
    public IEnumerable<string> StatusOptions =>
        UpdateCalendarViewModel.DayStatusOptions.Where(o => o != "Sunday" || IsSunday || DefaultStatus == "Sunday" || DayStatus == "Sunday");
}

public class CalendarMonthItem
{
    public int Value    { get; set; }
    public string Name  { get; set; } = string.Empty;
}
