using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.GroupShiftAssigns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class GroupShiftAssignController : Controller
{
    private readonly AttendanceDbContext _context;

    public GroupShiftAssignController(AttendanceDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var viewModel = new GroupShiftAssignViewModel();
        PopulateDropdownsAsync(viewModel).GetAwaiter().GetResult();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadGroupEmployees(int groupId)
    {
        var viewModel = new GroupShiftAssignViewModel { GroupId = groupId };
        await PopulateDropdownsAsync(viewModel);
        await LoadGroupEmployeesAsync(viewModel);
        return View("Index", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(GroupShiftAssignViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            await LoadGroupEmployeesAsync(viewModel);
            return View("Index", viewModel);
        }

        // Validate date is not back date
        if (viewModel.AssignedShiftDate.Date < DateTime.UtcNow.Date)
        {
            ModelState.AddModelError("AssignedShiftDate", "Assigned shift date cannot be a back date.");
            await PopulateDropdownsAsync(viewModel);
            await LoadGroupEmployeesAsync(viewModel);
            return View("Index", viewModel);
        }

        if (viewModel.SelectedEmployeeIds == null || !viewModel.SelectedEmployeeIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Please select at least one employee to assign shift.");
            await PopulateDropdownsAsync(viewModel);
            await LoadGroupEmployeesAsync(viewModel);
            return View("Index", viewModel);
        }

        // Update selected employees
        var employees = await _context.Employees
            .Where(e => viewModel.SelectedEmployeeIds.Contains(e.Id))
            .ToListAsync();

        foreach (var employee in employees)
        {
            employee.ShiftId = viewModel.AssignedShiftId;
            employee.AssignedShiftDate = viewModel.AssignedShiftDate;
        }

        _context.Employees.UpdateRange(employees);
        await _context.SaveChangesAsync();

        var assignedCount = employees.Count;
        TempData["SuccessMessage"] = $"Shift assigned successfully to {assignedCount} employee(s).";

        // Reset form
        var newViewModel = new GroupShiftAssignViewModel();
        await PopulateDropdownsAsync(newViewModel);
        return View("Index", newViewModel);
    }

    private async Task PopulateDropdownsAsync(GroupShiftAssignViewModel viewModel)
    {
        // Get current user's company - for now, use first company
        var company = await _context.Companies.OrderBy(c => c.Name).FirstOrDefaultAsync();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";

        viewModel.CompanyId = companyId;
        viewModel.CompanyName = companyName;

        // Populate groups
        var groups = await _context.Groups
            .Where(g => g.IsActive && g.CompanyId == companyId)
            .OrderBy(g => g.Name)
            .ToListAsync();
        viewModel.AvailableGroups = groups.Select(g => new GroupSelectItem
        {
            Id = g.Id,
            Name = g.Name,
            Code = g.Code
        }).ToList();

        // Populate shifts
        var shifts = await _context.Shifts
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
        viewModel.AvailableShifts = shifts.Select(s => new ShiftSelectItem
        {
            Id = s.Id,
            Name = s.Name
        }).ToList();
    }

    private async Task LoadGroupEmployeesAsync(GroupShiftAssignViewModel viewModel)
    {
        if (viewModel.GroupId == 0)
        {
            viewModel.GroupEmployees = new List<GroupEmployeeItem>();
            return;
        }

        // Get employees assigned to this group
        var groupEmployees = await _context.GroupEmployees
            .Include(ge => ge.Employee)
            .Where(ge => ge.GroupId == viewModel.GroupId && ge.IsActive)
            .ToListAsync();

        viewModel.GroupEmployees = groupEmployees.Select(ge => new GroupEmployeeItem
        {
            EmployeeId = ge.EmployeeId,
            EmployeeNo = ge.Employee?.EmployeeNo ?? string.Empty,
            Name = ge.Employee?.Name ?? string.Empty,
            CurrentShiftId = ge.Employee?.ShiftId,
            CurrentShiftName = ge.Employee?.Shift?.Name,
            CurrentAssignedShiftDate = ge.Employee?.AssignedShiftDate,
            IsSelected = false
        }).ToList();
    }
}
