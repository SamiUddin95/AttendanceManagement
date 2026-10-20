using Microsoft.EntityFrameworkCore;
using AttendanceManagement.Infrastructure.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Modules;
using AttendanceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Web.ViewComponents;

public class ModuleNavigationViewComponent : ViewComponent
{
    private readonly IUserModuleService _userModuleService;
    private readonly AttendanceDbContext _context;

    public ModuleNavigationViewComponent(IUserModuleService userModuleService, AttendanceDbContext context)
    {
        _userModuleService = userModuleService;
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var user = User as ClaimsPrincipal;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return View(new List<ModuleDto>());
        }

        if (user.IsInRole("SuperUser"))
        {
            var allModules = await _userModuleService.GetAllActiveModulesAsync(cancellationToken);
            return View(allModules);
        }

        var companyIdClaim = user.FindFirst("CompanyId");
        var isEmployeeClaim = user.FindFirst("IsEmployee");
        var assignmentType = isEmployeeClaim?.Value == "true" ? "Individual" : "Group";

        if (!Guid.TryParse(companyIdClaim?.Value, out var companyId))
        {
            return View(new List<ModuleDto>());
        }

        int? groupId = null;
        int? departmentId = null;
        int? employeeId = null;

        if (assignmentType == "Individual")
        {
            var nameIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (int.TryParse(nameIdClaim?.Value, out var empId))
            {
                employeeId = empId;
            }
        }

        var modules = await _userModuleService.GetUserModulesAsync(companyId, assignmentType, groupId, departmentId, employeeId, cancellationToken);
                var visible = modules.Where(m => m.Name != "SuperUser").ToList();
        var defaults = await _userModuleService.GetAllActiveModulesAsync(cancellationToken);
        foreach (var module in defaults.Where(m => m.Name == "GenEmployee" || m.Name == "GenIncharge"))
        {
            if (module.Name == "GenIncharge" && !await _context.Employees.AnyAsync(e => e.InchargeEmployeeId == employeeId && e.CompanyId == companyId && e.IsActive, cancellationToken)) continue;
            if (visible.All(m => m.Id != module.Id)) visible.Add(module);
        }
        return View(visible);
    }
}
