using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.ViewModels.GenIncharge;

public class ShiftSchedulingViewModel
{
    public int? SelectedEmployeeId { get; set; }
    public SelectList EmployeeOptions { get; set; } = new SelectList(new List<object>());
    public List<ShiftOptionItem> Shifts { get; set; } = new();
}

public class ShiftOptionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Timing { get; set; }
    public bool IsActive { get; set; }
    public string Display => string.IsNullOrWhiteSpace(Timing) ? Name : $"{Name} ({Timing})";
}

public class ShiftScheduleEntryDto
{
    public int? Id { get; set; }
    public int ShiftId { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SaveShiftScheduleRequest
{
    public int EmployeeId { get; set; }
    public List<ShiftScheduleEntryDto> Entries { get; set; } = new();
}

public class EmployeeShiftScheduleResponse
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNo { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public int? DefaultShiftId { get; set; }
    public string? DefaultShiftName { get; set; }
    public int? CurrentShiftId { get; set; }
    public List<ShiftScheduleRow> Entries { get; set; } = new();
}

public class ShiftScheduleRow
{
    public int Id { get; set; }
    public int ShiftId { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string EffectiveFrom { get; set; } = string.Empty;   // yyyy-MM-dd
    public string? EffectiveTo { get; set; }                    // yyyy-MM-dd, null = open-ended
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
