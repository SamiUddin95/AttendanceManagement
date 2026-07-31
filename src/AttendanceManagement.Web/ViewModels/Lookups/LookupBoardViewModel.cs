using System.Collections.Generic;

namespace AttendanceManagement.Web.ViewModels.Lookups;

public class LookupBoardViewModel
{
    public string ActiveSection { get; set; } = "Categories";
    public IList<LookupSectionViewModel> Sections { get; set; } = new List<LookupSectionViewModel>();
}

public class LookupSectionViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IList<LookupItemViewModel> Items { get; set; } = new List<LookupItemViewModel>();
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class LookupItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool? IsIncharge { get; set; }
}
