using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Web.ViewModels.Shifts;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
public class ShiftController : Controller
{
    private readonly IShiftService _shiftService;
    private readonly ICompanyService _companyService;

    public ShiftController(IShiftService shiftService, ICompanyService companyService)
    {
        _shiftService = shiftService;
        _companyService = companyService;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var (shifts, totalCount) = await _shiftService.GetShiftsAsync(page, 10, search);
        var viewModel = new ShiftListViewModel
        {
            Shifts = shifts.Select(s => new ShiftViewModel
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                CompanyName = s.Company.Name,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                ShiftDays = s.ShiftDays.Select(d => new ShiftDayViewModel
                {
                    Id = d.Id,
                    ShiftId = d.ShiftId,
                    DayOfWeek = (int)d.DayOfWeek,
                    DayName = d.DayOfWeek.ToString(),
                    IsWorking = d.IsWorking,
                    IsAlternate = d.IsAlternate,
                    IsHoliday = d.IsHoliday,
                    IsOther = d.IsOther,
                    IsSunday = d.IsSunday,
                    OtherHours = d.OtherHours,
                    OtherMinutes = d.OtherMinutes,
                    StartTimeHours = d.StartTime?.Hours,
                    StartTimeMinutes = d.StartTime?.Minutes,
                    DurationHours = d.Duration?.Hours,
                    DurationMinutes = d.Duration?.Minutes,
                    LunchBreakHours = d.LunchBreak?.Hours,
                    LunchBreakMinutes = d.LunchBreak?.Minutes,
                    PrayerBreakHours = d.PrayerBreak?.Hours,
                    PrayerBreakMinutes = d.PrayerBreak?.Minutes,
                    TeaBreakHours = d.TeaBreak?.Hours,
                    TeaBreakMinutes = d.TeaBreak?.Minutes,
                    FlexiInHours = d.FlexiIn?.Hours,
                    FlexiInMinutes = d.FlexiIn?.Minutes,
                    FlexiOutHours = d.FlexiOut?.Hours,
                    FlexiOutMinutes = d.FlexiOut?.Minutes,
                    IsFlexible = d.IsFlexible,
                    CopyFromPrevious = d.CopyFromPrevious
                }).ToList()
            }).ToList(),
            CurrentPage = page,
            PageSize = 10,
            TotalCount = totalCount,
            Search = search
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Details(int id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift == null) return NotFound();

        var viewModel = new ShiftViewModel
        {
            Id = shift.Id,
            CompanyId = shift.CompanyId,
            CompanyName = shift.Company.Name,
            Name = shift.Name,
            Description = shift.Description,
            IsActive = shift.IsActive,
            ShiftDays = shift.ShiftDays.Select(d => new ShiftDayViewModel
            {
                Id = d.Id,
                ShiftId = d.ShiftId,
                DayOfWeek = (int)d.DayOfWeek,
                DayName = d.DayOfWeek.ToString(),
                IsWorking = d.IsWorking,
                IsAlternate = d.IsAlternate,
                IsHoliday = d.IsHoliday,
                IsOther = d.IsOther,
                IsSunday = d.IsSunday,
                OtherHours = d.OtherHours,
                OtherMinutes = d.OtherMinutes,
                StartTimeHours = d.StartTime?.Hours,
                StartTimeMinutes = d.StartTime?.Minutes,
                DurationHours = d.Duration?.Hours,
                DurationMinutes = d.Duration?.Minutes,
                LunchBreakHours = d.LunchBreak?.Hours,
                LunchBreakMinutes = d.LunchBreak?.Minutes,
                PrayerBreakHours = d.PrayerBreak?.Hours,
                PrayerBreakMinutes = d.PrayerBreak?.Minutes,
                TeaBreakHours = d.TeaBreak?.Hours,
                TeaBreakMinutes = d.TeaBreak?.Minutes,
                FlexiInHours = d.FlexiIn?.Hours,
                FlexiInMinutes = d.FlexiIn?.Minutes,
                FlexiOutHours = d.FlexiOut?.Hours,
                FlexiOutMinutes = d.FlexiOut?.Minutes,
                IsFlexible = d.IsFlexible,
                CopyFromPrevious = d.CopyFromPrevious
            }).ToList()
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        // Get current user's company - for now, we'll use the first company as default
        // In a real scenario, this would come from the authenticated user's company
        var companies = await _companyService.GetAllAsync();
        var company = companies.FirstOrDefault();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";
        
        var viewModel = new ShiftViewModel
        {
            CompanyId = companyId,
            CompanyName = companyName,
            ShiftDays = Enum.GetValues<DayOfWeek>().Select(d => new ShiftDayViewModel
            {
                DayOfWeek = (int)d,
                DayName = d.ToString(),
                IsSunday = d == DayOfWeek.Sunday
            }).ToList()
        };
        ViewBag.CompanyId = companyId;
        ViewBag.CompanyName = companyName;
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ShiftViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var shift = new Shift
        {
            CompanyId = model.CompanyId,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive,
            ShiftDays = model.ShiftDays.Select(d => new ShiftDay
            {
                DayOfWeek = (DayOfWeek)d.DayOfWeek,
                IsWorking = d.IsWorking,
                IsAlternate = d.IsAlternate,
                IsHoliday = d.IsHoliday,
                IsOther = d.IsOther,
                IsSunday = d.IsSunday,
                OtherHours = d.OtherHours,
                OtherMinutes = d.OtherMinutes,
                StartTime = d.StartTimeHours.HasValue && d.StartTimeMinutes.HasValue 
                    ? TimeSpan.FromHours(d.StartTimeHours.Value) + TimeSpan.FromMinutes(d.StartTimeMinutes.Value) 
                    : null,
                Duration = d.DurationHours.HasValue && d.DurationMinutes.HasValue 
                    ? TimeSpan.FromHours(d.DurationHours.Value) + TimeSpan.FromMinutes(d.DurationMinutes.Value) 
                    : null,
                LunchBreak = d.LunchBreakHours.HasValue && d.LunchBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.LunchBreakHours.Value) + TimeSpan.FromMinutes(d.LunchBreakMinutes.Value) 
                    : null,
                PrayerBreak = d.PrayerBreakHours.HasValue && d.PrayerBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.PrayerBreakHours.Value) + TimeSpan.FromMinutes(d.PrayerBreakMinutes.Value) 
                    : null,
                TeaBreak = d.TeaBreakHours.HasValue && d.TeaBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.TeaBreakHours.Value) + TimeSpan.FromMinutes(d.TeaBreakMinutes.Value) 
                    : null,
                FlexiIn = d.FlexiInHours.HasValue && d.FlexiInMinutes.HasValue 
                    ? TimeSpan.FromHours(d.FlexiInHours.Value) + TimeSpan.FromMinutes(d.FlexiInMinutes.Value) 
                    : null,
                FlexiOut = d.FlexiOutHours.HasValue && d.FlexiOutMinutes.HasValue 
                    ? TimeSpan.FromHours(d.FlexiOutHours.Value) + TimeSpan.FromMinutes(d.FlexiOutMinutes.Value) 
                    : null,
                IsFlexible = d.IsFlexible,
                CopyFromPrevious = d.CopyFromPrevious
            }).ToList()
        };

        await _shiftService.CreateShiftAsync(shift);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift == null) return NotFound();

        var viewModel = new ShiftViewModel
        {
            Id = shift.Id,
            CompanyId = shift.CompanyId,
            CompanyName = shift.Company.Name,
            Name = shift.Name,
            Description = shift.Description,
            IsActive = shift.IsActive,
            ShiftDays = shift.ShiftDays.Select(d => new ShiftDayViewModel
            {
                Id = d.Id,
                ShiftId = d.ShiftId,
                DayOfWeek = (int)d.DayOfWeek,
                DayName = d.DayOfWeek.ToString(),
                IsWorking = d.IsWorking,
                IsAlternate = d.IsAlternate,
                IsHoliday = d.IsHoliday,
                IsOther = d.IsOther,
                IsSunday = d.IsSunday,
                OtherHours = d.OtherHours,
                OtherMinutes = d.OtherMinutes,
                StartTimeHours = d.StartTime?.Hours,
                StartTimeMinutes = d.StartTime?.Minutes,
                DurationHours = d.Duration?.Hours,
                DurationMinutes = d.Duration?.Minutes,
                LunchBreakHours = d.LunchBreak?.Hours,
                LunchBreakMinutes = d.LunchBreak?.Minutes,
                PrayerBreakHours = d.PrayerBreak?.Hours,
                PrayerBreakMinutes = d.PrayerBreak?.Minutes,
                TeaBreakHours = d.TeaBreak?.Hours,
                TeaBreakMinutes = d.TeaBreak?.Minutes,
                FlexiInHours = d.FlexiIn?.Hours,
                FlexiInMinutes = d.FlexiIn?.Minutes,
                FlexiOutHours = d.FlexiOut?.Hours,
                FlexiOutMinutes = d.FlexiOut?.Minutes,
                IsFlexible = d.IsFlexible,
                CopyFromPrevious = d.CopyFromPrevious
            }).ToList()
        };
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ShiftViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var shift = new Shift
        {
            Id = model.Id,
            CompanyId = model.CompanyId,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive,
            ShiftDays = model.ShiftDays.Select(d => new ShiftDay
            {
                Id = d.Id,
                ShiftId = d.ShiftId,
                DayOfWeek = (DayOfWeek)d.DayOfWeek,
                IsWorking = d.IsWorking,
                IsAlternate = d.IsAlternate,
                IsHoliday = d.IsHoliday,
                IsOther = d.IsOther,
                IsSunday = d.IsSunday,
                OtherHours = d.OtherHours,
                OtherMinutes = d.OtherMinutes,
                StartTime = d.StartTimeHours.HasValue && d.StartTimeMinutes.HasValue 
                    ? TimeSpan.FromHours(d.StartTimeHours.Value) + TimeSpan.FromMinutes(d.StartTimeMinutes.Value) 
                    : null,
                Duration = d.DurationHours.HasValue && d.DurationMinutes.HasValue 
                    ? TimeSpan.FromHours(d.DurationHours.Value) + TimeSpan.FromMinutes(d.DurationMinutes.Value) 
                    : null,
                LunchBreak = d.LunchBreakHours.HasValue && d.LunchBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.LunchBreakHours.Value) + TimeSpan.FromMinutes(d.LunchBreakMinutes.Value) 
                    : null,
                PrayerBreak = d.PrayerBreakHours.HasValue && d.PrayerBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.PrayerBreakHours.Value) + TimeSpan.FromMinutes(d.PrayerBreakMinutes.Value) 
                    : null,
                TeaBreak = d.TeaBreakHours.HasValue && d.TeaBreakMinutes.HasValue 
                    ? TimeSpan.FromHours(d.TeaBreakHours.Value) + TimeSpan.FromMinutes(d.TeaBreakMinutes.Value) 
                    : null,
                FlexiIn = d.FlexiInHours.HasValue && d.FlexiInMinutes.HasValue 
                    ? TimeSpan.FromHours(d.FlexiInHours.Value) + TimeSpan.FromMinutes(d.FlexiInMinutes.Value) 
                    : null,
                FlexiOut = d.FlexiOutHours.HasValue && d.FlexiOutMinutes.HasValue 
                    ? TimeSpan.FromHours(d.FlexiOutHours.Value) + TimeSpan.FromMinutes(d.FlexiOutMinutes.Value) 
                    : null,
                IsFlexible = d.IsFlexible,
                CopyFromPrevious = d.CopyFromPrevious
            }).ToList()
        };

        await _shiftService.UpdateShiftAsync(shift);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _shiftService.DeleteShiftAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _shiftService.ToggleShiftStatusAsync(id);
        return Json(new { success = result });
    }
}
