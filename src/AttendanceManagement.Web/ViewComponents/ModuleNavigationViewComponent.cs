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

    public ModuleNavigationViewComponent(IUserModuleService userModuleService)
    {
        _userModuleService = userModuleService;
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
        return View(modules);
    }
}
