namespace AttendanceManagement.Domain.Entities;

public class ShiftDay
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    
    // Day Type
    public bool IsWorking { get; set; }
    public bool IsAlternate { get; set; }
    public bool IsHoliday { get; set; }
    public bool IsOther { get; set; }
    public bool IsSunday { get; set; }
    public int? OtherHours { get; set; }
    public int? OtherMinutes { get; set; }
    
    // Time Configuration
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public TimeSpan? LunchBreak { get; set; }
    public TimeSpan? PrayerBreak { get; set; }
    public TimeSpan? TeaBreak { get; set; }
    public TimeSpan? FlexiIn { get; set; }
    public TimeSpan? FlexiOut { get; set; }
    public bool IsFlexible { get; set; }
    
    // Copy from previous day
    public bool CopyFromPrevious { get; set; }

    public Shift Shift { get; set; } = null!;
}
