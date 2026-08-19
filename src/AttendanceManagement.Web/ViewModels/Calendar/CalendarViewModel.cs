using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.Calendar;

public class CalendarViewModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public int CurrentMonth { get; set; }
    public int CurrentYear { get; set; }
    public List<CalendarEvent> Events { get; set; } = new();
    public List<int> AvailableYears { get; set; } = new();
    public List<MonthItem> AvailableMonths { get; set; } = new();
    public SelectList MonthOptions { get; set; } = new SelectList(new List<SelectListItem>());
    public SelectList YearOptions { get; set; } = new SelectList(new List<SelectListItem>());
}

public class CalendarEvent
{
    public DateTime Date { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Holiday, Leave, Birthday, etc.
    public string Color { get; set; } = "#0d6efd";
    public string? Description { get; set; }
}

public class MonthItem
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
}
