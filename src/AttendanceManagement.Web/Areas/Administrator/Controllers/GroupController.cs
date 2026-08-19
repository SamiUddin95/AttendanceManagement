using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class GroupController : Controller
{
    private readonly AttendanceDbContext _context;

    public GroupController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Groups.Include(g => g.Company).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(g => g.Name.ToLower().Contains(searchLower) ||
                                     g.Code.ToLower().Contains(searchLower));
        }

        var groups = await query.OrderByDescending(g => g.CreatedAtUtc).ToListAsync();
        var viewModels = groups.Select(g => new GroupViewModel
        {
            Id = g.Id,
            CompanyId = g.CompanyId,
            CompanyName = g.Company?.Name,
            Name = g.Name,
            Code = g.Code,
            Description = g.Description,
            EmploymentType = g.EmploymentType,
            Gender = g.Gender,
            IsActive = g.IsActive,
            FilterByDepartment = g.FilterByDepartment,
            FilterByDesignation = g.FilterByDesignation,
            FilterByLocation = g.FilterByLocation,
            FilterByIncharge = g.FilterByIncharge
        }).ToList();

        var listViewModel = new GroupListViewModel
        {
            Groups = viewModels,
            Search = search
        };

        return View(listViewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new GroupViewModel();
        await PopulateDropdownsAsync(viewModel);
        await LoadGroupEmployeesAsync(viewModel);
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GroupViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(viewModel);
            await LoadGroupEmployeesAsync(viewModel);
            return View("Edit", viewModel);
        }

        var group = new Group
        {
            CompanyId = viewModel.CompanyId,
            Name = viewModel.Name,
            Code = viewModel.Code,
            Description = viewModel.Description,
            EmploymentType = viewModel.EmploymentType,
            Gender = viewModel.Gender,
            JoiningDateFrom = viewModel.JoiningDateFrom,
            JoiningDateTo = viewModel.JoiningDateTo,
            PeriodEndDateFrom = viewModel.PeriodEndDateFrom,
            PeriodEndDateTo = viewModel.PeriodEndDateTo,
            ResignationDateFrom = viewModel.ResignationDateFrom,
            ResignationDateTo = viewModel.ResignationDateTo,
            DateOfBirthFrom = viewModel.DateOfBirthFrom,
            DateOfBirthTo = viewModel.DateOfBirthTo,
            FilterByDepartment = viewModel.FilterByDepartment,
            FilterByDesignation = viewModel.FilterByDesignation,
            FilterByLocation = viewModel.FilterByLocation,
            FilterByIncharge = viewModel.FilterByIncharge,
            IsActive = viewModel.IsActive
        };

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        // Add junction table entries
        await AddJunctionEntriesAsync(group.Id, viewModel);

        // Add group employees
        await AddGroupEmployeesAsync(group.Id, viewModel);

        TempData["SuccessMessage"] = "Group created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Company)
            .Include(g => g.GroupDepartments)
            .Include(g => g.GroupLocations)
            .Include(g => g.GroupDesignations)
            .Include(g => g.GroupIncharges)
            .Include(g => g.GroupEmployees)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group is null)
        {
            return NotFound();
        }

        var viewModel = new GroupViewModel
        {
            Id = group.Id,
            CompanyId = group.CompanyId,
            CompanyName = group.Company?.Name,
            Name = group.Name,
            Code = group.Code,
            Description = group.Description,
            EmploymentType = group.EmploymentType,
            Gender = group.Gender,
            JoiningDateFrom = group.JoiningDateFrom,
            JoiningDateTo = group.JoiningDateTo,
            PeriodEndDateFrom = group.PeriodEndDateFrom,
            PeriodEndDateTo = group.PeriodEndDateTo,
            ResignationDateFrom = group.ResignationDateFrom,
            ResignationDateTo = group.ResignationDateTo,
            DateOfBirthFrom = group.DateOfBirthFrom,
            DateOfBirthTo = group.DateOfBirthTo,
            FilterByDepartment = group.FilterByDepartment,
            FilterByDesignation = group.FilterByDesignation,
            FilterByLocation = group.FilterByLocation,
            FilterByIncharge = group.FilterByIncharge,
            IsActive = group.IsActive,
            SelectedDepartmentIds = group.GroupDepartments.Select(gd => gd.DepartmentId).ToList(),
            SelectedLocationIds = group.GroupLocations.Select(gl => gl.LocationId).ToList(),
            SelectedDesignationIds = group.GroupDesignations.Select(gd => gd.DesignationId).ToList(),
            SelectedInchargeIds = group.GroupIncharges.Select(gi => gi.EmployeeId).ToList(),
            SelectedEmployeeIds = group.GroupEmployees.Where(ge => ge.IsActive).Select(ge => ge.EmployeeId).ToList()
        };

        await PopulateDropdownsAsync(viewModel);
        await LoadGroupEmployeesAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GroupViewModel viewModel)
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

        var group = await _context.Groups.FindAsync(id);
        if (group is null)
        {
            return NotFound();
        }

        group.Name = viewModel.Name;
        group.Code = viewModel.Code;
        group.Description = viewModel.Description;
        group.EmploymentType = viewModel.EmploymentType;
        group.Gender = viewModel.Gender;
        group.JoiningDateFrom = viewModel.JoiningDateFrom;
        group.JoiningDateTo = viewModel.JoiningDateTo;
        group.PeriodEndDateFrom = viewModel.PeriodEndDateFrom;
        group.PeriodEndDateTo = viewModel.PeriodEndDateTo;
        group.ResignationDateFrom = viewModel.ResignationDateFrom;
        group.ResignationDateTo = viewModel.ResignationDateTo;
        group.DateOfBirthFrom = viewModel.DateOfBirthFrom;
        group.DateOfBirthTo = viewModel.DateOfBirthTo;
        group.FilterByDepartment = viewModel.FilterByDepartment;
        group.FilterByDesignation = viewModel.FilterByDesignation;
        group.FilterByLocation = viewModel.FilterByLocation;
        group.FilterByIncharge = viewModel.FilterByIncharge;
        group.IsActive = viewModel.IsActive;
        group.UpdatedAtUtc = DateTime.UtcNow;

        // Remove existing junction entries
        await RemoveJunctionEntriesAsync(id);

        // Add new junction entries
        await AddJunctionEntriesAsync(id, viewModel);

        // Update group employees
        await UpdateGroupEmployeesAsync(id, viewModel);

        _context.Groups.Update(group);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Group updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group is null)
        {
            return NotFound();
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Group deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(GroupViewModel viewModel)
    {
        // Get current user's company - for now, use first company
        var company = await _context.Companies.OrderBy(c => c.Name).FirstOrDefaultAsync();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";

        viewModel.CompanyId = companyId;
        viewModel.CompanyName = companyName;

        // Populate departments
        var departments = await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
        viewModel.AvailableDepartments = departments.Select(d => new DepartmentSelectItem
        {
            Id = d.Id,
            Name = d.Name,
            IsSelected = viewModel.SelectedDepartmentIds.Contains(d.Id)
        }).ToList();

        // Populate locations
        var locations = await _context.Locations
            .Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .ToListAsync();
        viewModel.AvailableLocations = locations.Select(l => new LocationSelectItem
        {
            Id = l.Id,
            Name = l.Name,
            IsSelected = viewModel.SelectedLocationIds.Contains(l.Id)
        }).ToList();

        // Populate designations
        var designations = await _context.Designations
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync();
        viewModel.AvailableDesignations = designations.Select(d => new DesignationSelectItem
        {
            Id = d.Id,
            Name = d.Name,
            IsSelected = viewModel.SelectedDesignationIds.Contains(d.Id)
        }).ToList();

        // Populate incharges (employees)
        var employees = await _context.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();
        viewModel.AvailableIncharges = employees.Select(e => new EmployeeSelectItem
        {
            Id = e.Id,
            Name = e.Name,
            EmployeeNo = e.EmployeeNo ?? string.Empty,
            IsSelected = viewModel.SelectedInchargeIds.Contains(e.Id)
        }).ToList();
    }

    private async Task AddJunctionEntriesAsync(int groupId, GroupViewModel viewModel)
    {
        // Add departments
        foreach (var deptId in viewModel.SelectedDepartmentIds)
        {
            _context.GroupDepartments.Add(new GroupDepartment
            {
                GroupId = groupId,
                DepartmentId = deptId
            });
        }

        // Add locations
        foreach (var locationId in viewModel.SelectedLocationIds)
        {
            _context.GroupLocations.Add(new GroupLocation
            {
                GroupId = groupId,
                LocationId = locationId
            });
        }

        // Add designations
        foreach (var designationId in viewModel.SelectedDesignationIds)
        {
            _context.GroupDesignations.Add(new GroupDesignation
            {
                GroupId = groupId,
                DesignationId = designationId
            });
        }

        // Add incharges
        foreach (var inchargeId in viewModel.SelectedInchargeIds)
        {
            _context.GroupIncharges.Add(new GroupIncharge
            {
                GroupId = groupId,
                EmployeeId = inchargeId
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task RemoveJunctionEntriesAsync(int groupId)
    {
        var departments = await _context.GroupDepartments.Where(gd => gd.GroupId == groupId).ToListAsync();
        _context.GroupDepartments.RemoveRange(departments);

        var locations = await _context.GroupLocations.Where(gl => gl.GroupId == groupId).ToListAsync();
        _context.GroupLocations.RemoveRange(locations);

        var designations = await _context.GroupDesignations.Where(gd => gd.GroupId == groupId).ToListAsync();
        _context.GroupDesignations.RemoveRange(designations);

        var incharges = await _context.GroupIncharges.Where(gi => gi.GroupId == groupId).ToListAsync();
        _context.GroupIncharges.RemoveRange(incharges);

        await _context.SaveChangesAsync();
    }

    private async Task LoadGroupEmployeesAsync(GroupViewModel viewModel)
    {
        if (viewModel.Id == 0)
        {
            // For new groups, load all active employees
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.IsActive && e.CompanyId == viewModel.CompanyId)
                .OrderBy(e => e.Name)
                .ToListAsync();

            viewModel.GroupEmployees = employees.Select(e => new GroupEmployeeItem
            {
                EmployeeId = e.Id,
                EmployeeNo = e.EmployeeNo,
                Name = e.Name,
                Department = e.Department?.Name,
                Designation = e.Designation?.Name
            }).ToList();
        }
        else
        {
            // For existing groups, load all active employees (not just assigned ones)
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .Where(e => e.IsActive && e.CompanyId == viewModel.CompanyId)
                .OrderBy(e => e.Name)
                .ToListAsync();

            viewModel.GroupEmployees = employees.Select(e => new GroupEmployeeItem
            {
                EmployeeId = e.Id,
                EmployeeNo = e.EmployeeNo,
                Name = e.Name,
                Department = e.Department?.Name,
                Designation = e.Designation?.Name
            }).ToList();
        }
    }

    private async Task UpdateGroupEmployeesAsync(int groupId, GroupViewModel viewModel)
    {
        // Get currently assigned employees
        var currentAssignments = await _context.GroupEmployees
            .Where(ge => ge.GroupId == groupId && ge.IsActive)
            .ToListAsync();

        // Get selected employee IDs
        var selectedIds = viewModel.SelectedEmployeeIds ?? new List<int>();

        // Remove employees that are no longer selected
        var toRemove = currentAssignments.Where(ge => !selectedIds.Contains(ge.EmployeeId)).ToList();
        foreach (var assignment in toRemove)
        {
            assignment.IsActive = false;
            assignment.RemovedAtUtc = DateTime.UtcNow;
        }

        // Add newly selected employees
        var currentIds = currentAssignments.Select(ge => ge.EmployeeId).ToList();
        var toAdd = selectedIds.Where(id => !currentIds.Contains(id)).ToList();
        foreach (var employeeId in toAdd)
        {
            _context.GroupEmployees.Add(new GroupEmployee
            {
                GroupId = groupId,
                EmployeeId = employeeId,
                IsActive = true,
                AssignedAtUtc = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task AddGroupEmployeesAsync(int groupId, GroupViewModel viewModel)
    {
        var selectedIds = viewModel.SelectedEmployeeIds ?? new List<int>();
        foreach (var employeeId in selectedIds)
        {
            _context.GroupEmployees.Add(new GroupEmployee
            {
                GroupId = groupId,
                EmployeeId = employeeId,
                IsActive = true,
                AssignedAtUtc = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
    }
}
