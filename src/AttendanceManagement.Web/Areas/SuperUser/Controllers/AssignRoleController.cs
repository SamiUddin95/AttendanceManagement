using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.SuperUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.SuperUser.Controllers;

[Area("SuperUser")]
[Authorize(Roles = "SuperUser,Administrator")]
public class AssignRoleController : Controller
{
    private readonly AttendanceDbContext _context;

    public AssignRoleController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var assignments = await _context.RoleAssignments
            .Include(ra => ra.Role)
            .Include(ra => ra.Company)
            .Include(ra => ra.Group)
            .Include(ra => ra.Department)
            .Include(ra => ra.Employee)
            .OrderBy(ra => ra.Company.Name)
            .ThenBy(ra => ra.AssignmentType)
            .ThenBy(ra => ra.Role.Name)
            .ToListAsync();

        var viewModel = new AssignRoleListViewModel
        {
            Assignments = assignments.Select(ra => new AssignRoleViewModel
            {
                Id = ra.Id,
                AssignmentType = ra.AssignmentType,
                RoleId = ra.RoleId,
                CompanyId = ra.CompanyId,
                GroupId = ra.GroupId,
                DepartmentId = ra.DepartmentId,
                EmployeeId = ra.EmployeeId,
                IsActive = ra.IsActive,
                RoleName = ra.Role?.Name ?? string.Empty,
                CompanyName = ra.Company?.Name ?? string.Empty,
                GroupName = ra.Group?.Name ?? string.Empty,
                DepartmentName = ra.Department?.Name ?? string.Empty,
                EmployeeName = ra.Employee?.Name ?? string.Empty,
                EmployeeNo = ra.Employee?.EmployeeNo ?? string.Empty
            }).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new AssignRoleViewModel
        {
            IsActive = true,
            AssignmentType = "Group"
        };

        await PopulateDropdowns(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssignRoleViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        // Validate based on assignment type
        if (viewModel.AssignmentType == "Group")
        {
            if (!viewModel.GroupId.HasValue)
            {
                ModelState.AddModelError("GroupId", "Group is required for Group assignment.");
            }
        }
        else if (viewModel.AssignmentType == "Individual")
        {
            if (!viewModel.DepartmentId.HasValue)
            {
                ModelState.AddModelError("DepartmentId", "Department is required for Individual assignment.");
            }
            if (!viewModel.EmployeeId.HasValue)
            {
                ModelState.AddModelError("EmployeeId", "Employee is required for Individual assignment.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        var assignment = new RoleAssignment
        {
            RoleId = viewModel.RoleId,
            CompanyId = viewModel.CompanyId,
            AssignmentType = viewModel.AssignmentType,
            GroupId = viewModel.AssignmentType == "Group" ? viewModel.GroupId : null,
            DepartmentId = viewModel.AssignmentType == "Individual" ? viewModel.DepartmentId : null,
            EmployeeId = viewModel.AssignmentType == "Individual" ? viewModel.EmployeeId : null,
            IsActive = viewModel.IsActive
        };

        _context.RoleAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role assignment created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var assignment = await _context.RoleAssignments
            .Include(ra => ra.Role)
            .Include(ra => ra.Company)
            .Include(ra => ra.Group)
            .Include(ra => ra.Department)
            .Include(ra => ra.Employee)
            .FirstOrDefaultAsync(ra => ra.Id == id);

        if (assignment == null)
        {
            return NotFound();
        }

        var viewModel = new AssignRoleViewModel
        {
            Id = assignment.Id,
            AssignmentType = assignment.AssignmentType,
            RoleId = assignment.RoleId,
            CompanyId = assignment.CompanyId,
            GroupId = assignment.GroupId,
            DepartmentId = assignment.DepartmentId,
            EmployeeId = assignment.EmployeeId,
            IsActive = assignment.IsActive,
            RoleName = assignment.Role?.Name ?? string.Empty,
            CompanyName = assignment.Company?.Name ?? string.Empty,
            GroupName = assignment.Group?.Name ?? string.Empty,
            DepartmentName = assignment.Department?.Name ?? string.Empty,
            EmployeeName = assignment.Employee?.Name ?? string.Empty,
            EmployeeNo = assignment.Employee?.EmployeeNo ?? string.Empty
        };

        await PopulateDropdowns(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AssignRoleViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        // Validate based on assignment type
        if (viewModel.AssignmentType == "Group")
        {
            if (!viewModel.GroupId.HasValue)
            {
                ModelState.AddModelError("GroupId", "Group is required for Group assignment.");
            }
        }
        else if (viewModel.AssignmentType == "Individual")
        {
            if (!viewModel.DepartmentId.HasValue)
            {
                ModelState.AddModelError("DepartmentId", "Department is required for Individual assignment.");
            }
            if (!viewModel.EmployeeId.HasValue)
            {
                ModelState.AddModelError("EmployeeId", "Employee is required for Individual assignment.");
            }
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(viewModel);
            return View(viewModel);
        }

        var assignment = await _context.RoleAssignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }

        assignment.RoleId = viewModel.RoleId;
        assignment.CompanyId = viewModel.CompanyId;
        assignment.AssignmentType = viewModel.AssignmentType;
        assignment.GroupId = viewModel.AssignmentType == "Group" ? viewModel.GroupId : null;
        assignment.DepartmentId = viewModel.AssignmentType == "Individual" ? viewModel.DepartmentId : null;
        assignment.EmployeeId = viewModel.AssignmentType == "Individual" ? viewModel.EmployeeId : null;
        assignment.IsActive = viewModel.IsActive;
        assignment.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role assignment updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await _context.RoleAssignments.FindAsync(id);
        if (assignment == null)
        {
            return NotFound();
        }

        _context.RoleAssignments.Remove(assignment);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role assignment deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetGroupsByCompany(Guid companyId)
    {
        var groups = await _context.Groups
            .Where(g => (g.CompanyId == companyId || g.CompanyId == Guid.Empty) && g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new { id = g.Id, name = g.Name })
            .ToListAsync();

        if (groups.Count == 0)
        {
            groups = await _context.Groups
                .Where(g => g.IsActive)
                .OrderBy(g => g.Name)
                .Select(g => new { id = g.Id, name = g.Name })
                .ToListAsync();
        }

        return Json(groups);
    }

    [HttpGet]
    public async Task<IActionResult> GetDepartmentsByCompany(Guid companyId)
    {
        var departments = await _context.Departments
            .Where(d => d.CompanyId == companyId || d.CompanyId == Guid.Empty)
            .OrderBy(d => d.Name)
            .Select(d => new { id = d.Id, name = d.Name })
            .ToListAsync();

        if (departments.Count == 0)
        {
            departments = await _context.Departments
                .OrderBy(d => d.Name)
                .Select(d => new { id = d.Id, name = d.Name })
                .ToListAsync();
        }

        return Json(departments);
    }

    [HttpGet]
    public async Task<IActionResult> GetEmployeesByDepartment(int departmentId)
    {
        var employees = await _context.Employees
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.Name)
            .Select(e => new { id = e.Id, name = e.Name, employeeNo = e.EmployeeNo })
            .ToListAsync();

        if (employees.Count == 0)
        {
            employees = await _context.Employees
                .OrderBy(e => e.Name)
                .Select(e => new { id = e.Id, name = e.Name, employeeNo = e.EmployeeNo })
                .ToListAsync();
        }

        return Json(employees);
    }

    private async Task PopulateDropdowns(AssignRoleViewModel viewModel)
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var companies = await _context.Companies
            .OrderBy(c => c.Name)
            .ToListAsync();

        viewModel.RoleOptions = new SelectList(roles, "Id", "Name", viewModel.RoleId);
        viewModel.CompanyOptions = new SelectList(companies, "Id", "Name", viewModel.CompanyId);

        if (viewModel.CompanyId != Guid.Empty)
        {
            var groups = await _context.Groups
                .Where(g => g.CompanyId == viewModel.CompanyId && g.IsActive)
                .OrderBy(g => g.Name)
                .ToListAsync();

            var departments = await _context.Departments
                .Where(d => d.CompanyId == viewModel.CompanyId && d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

            viewModel.GroupOptions = new SelectList(groups, "Id", "Name", viewModel.GroupId);
            viewModel.DepartmentOptions = new SelectList(departments, "Id", "Name", viewModel.DepartmentId);

            if (viewModel.DepartmentId.HasValue)
            {
                var employees = await _context.Employees
                    .Where(e => e.DepartmentId == viewModel.DepartmentId && e.IsActive)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                viewModel.EmployeeOptions = new SelectList(employees, "Id", "Name", viewModel.EmployeeId);
            }
        }
    }
}
