using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.MarkDays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class MarkDayController : Controller
{
    private readonly AttendanceDbContext _context;

    public MarkDayController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.MarkDays
            .Include(md => md.Company)
            .Include(md => md.Group)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(md => md.Group.Name.ToLower().Contains(searchLower) ||
                                     md.Group.Code.ToLower().Contains(searchLower));
        }

        var markDays = await query.OrderByDescending(md => md.CreatedAtUtc).ToListAsync();
        var viewModels = markDays.Select(md => new MarkDayViewModel
        {
            Id = md.Id,
            CompanyId = md.CompanyId,
            CompanyName = md.Company?.Name,
            GroupId = md.GroupId,
            GroupName = md.Group?.Name,
            FromDate = md.FromDate,
            ToDate = md.ToDate,
            TotalDays = md.TotalDays,
            IsOn = md.IsOn,
            IsOff = md.IsOff,
            IsGazettedHoliday = md.IsGazettedHoliday,
            IsProvincialHoliday = md.IsProvincialHoliday,
            IsStrike = md.IsStrike,
            IsEid = md.IsEid
        }).ToList();

        var listViewModel = new MarkDayListViewModel
        {
            MarkDays = viewModels,
            Search = search
        };

        return View(listViewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new MarkDayViewModel();
        await PopulateDropdownsAsync(viewModel);
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MarkDayViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            return View("Edit", viewModel);
        }

        // Calculate total days
        viewModel.TotalDays = (viewModel.ToDate - viewModel.FromDate).Days + 1;

        // Count affected employees (excluding inactive/resigned)
        var affectedEmployees = await _context.Employees
            .Where(e => e.CompanyId == viewModel.CompanyId && 
                       e.IsActive && 
                       e.ResignationDate == null)
            .CountAsync();
        viewModel.AffectedEmployeesCount = affectedEmployees;

        var markDay = new MarkDay
        {
            CompanyId = viewModel.CompanyId,
            GroupId = viewModel.GroupId,
            FromDate = viewModel.FromDate,
            ToDate = viewModel.ToDate,
            TotalDays = viewModel.TotalDays,
            IsOn = viewModel.IsOn,
            IsOff = viewModel.IsOff,
            IsGazettedHoliday = viewModel.IsGazettedHoliday,
            IsProvincialHoliday = viewModel.IsProvincialHoliday,
            IsStrike = viewModel.IsStrike,
            IsEid = viewModel.IsEid
        };

        _context.MarkDays.Add(markDay);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Day marking created successfully. {affectedEmployees} active employees will be affected.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var markDay = await _context.MarkDays
            .Include(md => md.Company)
            .Include(md => md.Group)
            .FirstOrDefaultAsync(md => md.Id == id);

        if (markDay is null)
        {
            return NotFound();
        }

        var viewModel = new MarkDayViewModel
        {
            Id = markDay.Id,
            CompanyId = markDay.CompanyId,
            CompanyName = markDay.Company?.Name,
            GroupId = markDay.GroupId,
            GroupName = markDay.Group?.Name,
            FromDate = markDay.FromDate,
            ToDate = markDay.ToDate,
            TotalDays = markDay.TotalDays,
            IsOn = markDay.IsOn,
            IsOff = markDay.IsOff,
            IsGazettedHoliday = markDay.IsGazettedHoliday,
            IsProvincialHoliday = markDay.IsProvincialHoliday,
            IsStrike = markDay.IsStrike,
            IsEid = markDay.IsEid
        };

        await PopulateDropdownsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MarkDayViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // Recalculate total days
        viewModel.TotalDays = (viewModel.ToDate - viewModel.FromDate).Days + 1;

        var markDay = await _context.MarkDays.FindAsync(id);
        if (markDay is null)
        {
            return NotFound();
        }

        markDay.GroupId = viewModel.GroupId;
        markDay.FromDate = viewModel.FromDate;
        markDay.ToDate = viewModel.ToDate;
        markDay.TotalDays = viewModel.TotalDays;
        markDay.IsOn = viewModel.IsOn;
        markDay.IsOff = viewModel.IsOff;
        markDay.IsGazettedHoliday = viewModel.IsGazettedHoliday;
        markDay.IsProvincialHoliday = viewModel.IsProvincialHoliday;
        markDay.IsStrike = viewModel.IsStrike;
        markDay.IsEid = viewModel.IsEid;
        markDay.UpdatedAtUtc = DateTime.UtcNow;

        _context.MarkDays.Update(markDay);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Day marking updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var markDay = await _context.MarkDays.FindAsync(id);
        if (markDay is null)
        {
            return NotFound();
        }

        _context.MarkDays.Remove(markDay);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Day marking deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(MarkDayViewModel viewModel)
    {
        // Get current user's company - for now, use first company
        var company = await _context.Companies.OrderBy(c => c.Name).FirstOrDefaultAsync();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";

        viewModel.CompanyId = companyId;
        viewModel.CompanyName = companyName;

        // Populate groups
        var groups = await _context.Groups
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .ToListAsync();
        viewModel.AvailableGroups = groups.Select(g => new GroupSelectItem
        {
            Id = g.Id,
            Name = g.Name,
            Code = g.Code
        }).ToList();
    }
}
