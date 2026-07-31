namespace AttendanceManagement.Web.ViewModels.Shifts;

public class ShiftViewModel
{
    public int Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<ShiftDayViewModel> ShiftDays { get; set; } = new();
}

public class ShiftDayViewModel
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    
    // Day Type
    public bool IsWorking { get; set; }
    public bool IsAlternate { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsOther { get; set; }
    public bool IsSunday { get; set; }
    public int? OtherHours { get; set; }
    public int? OtherMinutes { get; set; }
    
    // Time Configuration
    public int? StartTimeHours { get; set; }
    public int? StartTimeMinutes { get; set; }
    public int? DurationHours { get; set; }
    public int? DurationMinutes { get; set; }
    public int? LunchBreakHours { get; set; }
    public int? LunchBreakMinutes { get; set; }
    public int? PrayerBreakHours { get; set; }
    public int? PrayerBreakMinutes { get; set; }
    public int? TeaBreakHours { get; set; }
    public int? TeaBreakMinutes { get; set; }
    public int? FlexiInHours { get; set; }
    public int? FlexiInMinutes { get; set; }
    public int? FlexiOutHours { get; set; }
    public int? FlexiOutMinutes { get; set; }
    public bool IsFlexible { get; set; }
    
    // Copy from previous day
    public bool CopyFromPrevious { get; set; }
}

public class ShiftListViewModel
{
    public List<ShiftViewModel> Shifts { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string? Search { get; set; }
}
