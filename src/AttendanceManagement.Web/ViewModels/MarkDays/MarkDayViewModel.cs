using System.ComponentModel.DataAnnotations;

namespace AttendanceManagement.Web.ViewModels.MarkDays;

public class MarkDayViewModel
{
    public int Id { get; set; }

    [Required]
    public Guid CompanyId { get; set; }

    public string? CompanyName { get; set; }

    [Required]
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    public int TotalDays { get; set; }

    // Status checkboxes
    public bool IsOn { get; set; } = false;
    public bool IsOff { get; set; } = false;
    public bool IsGazettedHoliday { get; set; } = false;
    public bool IsProvincialHoliday { get; set; } = false;
    public bool IsStrike { get; set; } = false;
    public bool IsEid { get; set; } = false;

    // Dropdown options
    public List<GroupSelectItem> AvailableGroups { get; set; } = new();
    public int AffectedEmployeesCount { get; set; }
}

public class GroupSelectItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class MarkDayListViewModel
{
    public List<MarkDayViewModel> MarkDays { get; set; } = new();
    public string? Search { get; set; }
}
