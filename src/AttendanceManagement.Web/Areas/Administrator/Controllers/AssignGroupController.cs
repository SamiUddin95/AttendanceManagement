using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.AssignGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class AssignGroupController : Controller
{
    private readonly AttendanceDbContext _context;

    public AssignGroupController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.AssignGroups
            .Include(ag => ag.Company)
            .Include(ag => ag.Group)
            .Include(ag => ag.Location)
            .Include(ag => ag.Department)
            .Include(ag => ag.InchargeCategory)
            .Include(ag => ag.InchargeDesignation)
            .Include(ag => ag.InchargeEmployee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(ag => ag.Group.Name.ToLower().Contains(searchLower) ||
                                     ag.Group.Code.ToLower().Contains(searchLower));
        }

        var assignGroups = await query.OrderByDescending(ag => ag.CreatedAtUtc).ToListAsync();
        var viewModels = assignGroups.Select(ag => new AssignGroupViewModel
        {
            Id = ag.Id,
            CompanyId = ag.CompanyId,
            CompanyName = ag.Company?.Name,
            GroupId = ag.GroupId,
            GroupName = ag.Group?.Name,
            EmployeeType = ag.EmployeeType,
            LocationId = ag.LocationId,
            DepartmentId = ag.DepartmentId,
            InchargeCategoryId = ag.InchargeCategoryId,
            InchargeDesignationId = ag.InchargeDesignationId,
            InchargeEmployeeId = ag.InchargeEmployeeId,
            IsActive = ag.IsActive
        }).ToList();

        var listViewModel = new AssignGroupListViewModel
        {
            AssignGroups = viewModels,
            Search = search
        };

        return View(listViewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new AssignGroupViewModel();
        await PopulateDropdownsAsync(viewModel);
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignGroupViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            return View("Edit", viewModel);
        }

        var assignGroup = new AssignGroup
        {
            CompanyId = viewModel.CompanyId,
            GroupId = viewModel.GroupId,
            EmployeeType = viewModel.EmployeeType,
            LocationId = viewModel.LocationId,
            DepartmentId = viewModel.DepartmentId,
            InchargeCategoryId = viewModel.InchargeCategoryId,
            InchargeDesignationId = viewModel.InchargeDesignationId,
            InchargeEmployeeId = viewModel.InchargeEmployeeId,
            IsActive = viewModel.IsActive
        };

        _context.AssignGroups.Add(assignGroup);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Group assignment created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var assignGroup = await _context.AssignGroups
            .Include(ag => ag.Company)
            .Include(ag => ag.Group)
            .Include(ag => ag.Location)
            .Include(ag => ag.Department)
            .Include(ag => ag.InchargeCategory)
            .Include(ag => ag.InchargeDesignation)
            .Include(ag => ag.InchargeEmployee)
            .FirstOrDefaultAsync(ag => ag.Id == id);

        if (assignGroup is null)
        {
            return NotFound();
        }

        var viewModel = new AssignGroupViewModel
        {
            Id = assignGroup.Id,
            CompanyId = assignGroup.CompanyId,
            CompanyName = assignGroup.Company?.Name,
            GroupId = assignGroup.GroupId,
            GroupName = assignGroup.Group?.Name,
            EmployeeType = assignGroup.EmployeeType,
            LocationId = assignGroup.LocationId,
            DepartmentId = assignGroup.DepartmentId,
            InchargeCategoryId = assignGroup.InchargeCategoryId,
            InchargeDesignationId = assignGroup.InchargeDesignationId,
            InchargeEmployeeId = assignGroup.InchargeEmployeeId,
            IsActive = assignGroup.IsActive
        };

        await PopulateDropdownsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssignGroupViewModel viewModel)
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

        var assignGroup = await _context.AssignGroups.FindAsync(id);
        if (assignGroup is null)
        {
            return NotFound();
        }

        assignGroup.GroupId = viewModel.GroupId;
        assignGroup.EmployeeType = viewModel.EmployeeType;
        assignGroup.LocationId = viewModel.LocationId;
        assignGroup.DepartmentId = viewModel.DepartmentId;
        assignGroup.InchargeCategoryId = viewModel.InchargeCategoryId;
        assignGroup.InchargeDesignationId = viewModel.InchargeDesignationId;
        assignGroup.InchargeEmployeeId = viewModel.InchargeEmployeeId;
        assignGroup.IsActive = viewModel.IsActive;
        assignGroup.UpdatedAtUtc = DateTime.UtcNow;

        _context.AssignGroups.Update(assignGroup);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Group assignment updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var assignGroup = await _context.AssignGroups.FindAsync(id);
        if (assignGroup is null)
        {
            return NotFound();
        }

        _context.AssignGroups.Remove(assignGroup);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Group assignment deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(AssignGroupViewModel viewModel)
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

        // Populate locations
        var locations = await _context.Locations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();
        viewModel.AvailableLocations = locations.Select(l => new LocationSelectItem
        {
            Id = l.Id,
            Name = l.Name
        }).ToList();

        // Populate departments
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
        viewModel.AvailableDepartments = departments.Select(d => new DepartmentSelectItem
        {
            Id = d.Id,
            Name = d.Name
        }).ToList();

        // Populate incharge categories
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        viewModel.AvailableInchargeCategories = categories.Select(c => new CategorySelectItem
        {
            Id = c.Id,
            Name = c.Name
        }).ToList();

        // Populate incharge designations
        var designations = await _context.Designations
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
        viewModel.AvailableInchargeDesignations = designations.Select(d => new DesignationSelectItem
        {
            Id = d.Id,
            Name = d.Name
        }).ToList();

        // Populate incharge employees
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
        viewModel.AvailableInchargeEmployees = employees.Select(e => new EmployeeSelectItem
        {
            Id = e.Id,
            Name = e.Name,
            EmployeeNo = e.EmployeeNo ?? string.Empty
        }).ToList();
    }
}
