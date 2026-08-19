using AttendanceManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.SuperUser.Controllers;

[Area("SuperUser")]
[Authorize(Roles = "SuperUser,Administrator")]
public class DashboardController : Controller
{
    private readonly AttendanceDbContext _context;

    public DashboardController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalCompanies = await _context.Companies.CountAsync();
        var totalRoles = await _context.Roles.CountAsync(r => r.IsActive);
        var totalModules = await _context.Modules.CountAsync(m => m.IsActive);
        var totalAssignments = await _context.RoleAssignments.CountAsync(ra => ra.IsActive);
        var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);

        ViewData["TotalCompanies"] = totalCompanies;
        ViewData["TotalRoles"] = totalRoles;
        ViewData["TotalModules"] = totalModules;
        ViewData["TotalAssignments"] = totalAssignments;
        ViewData["TotalEmployees"] = totalEmployees;

        return View();
    }
}
