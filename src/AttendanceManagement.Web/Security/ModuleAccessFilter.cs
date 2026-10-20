using System.Security.Claims;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Security;

// Enforce permissions on requests, not only in the navigation menu.
public sealed class ModuleAccessFilter(AttendanceDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var pair in context.HttpContext.Request.Query)
        {
            if (pair.Key.Equals("month", StringComparison.OrdinalIgnoreCase) && (!int.TryParse(pair.Value, out var month) || month < 1 || month > 12)
                || pair.Key.Equals("year", StringComparison.OrdinalIgnoreCase) && (!int.TryParse(pair.Value, out var year) || year < 2000 || year > 2100))
            { context.Result = new BadRequestObjectResult("Choose a valid month and a year between 2000 and 2100."); return; }
        }
        if (context.HttpContext.Request.HasFormContentType)
        {
            foreach (var pair in context.HttpContext.Request.Form)
            {
                if ((pair.Key.Equals("SelectedMonth", StringComparison.OrdinalIgnoreCase) || pair.Key == "month") && (!int.TryParse(pair.Value, out var month) || month < 1 || month > 12)
                    || (pair.Key.Equals("SelectedYear", StringComparison.OrdinalIgnoreCase) || pair.Key == "year") && (!int.TryParse(pair.Value, out var year) || year < 2000 || year > 2100))
                { context.Result = new BadRequestObjectResult("Invalid reporting period."); return; }
            }
        }
        var user = context.HttpContext.User;
        var area = context.RouteData.Values["area"]?.ToString();
        if (string.IsNullOrEmpty(area)) { await next(); return; }
        if (user.Identity?.IsAuthenticated != true) { context.Result = new ChallengeResult(); return; }
        if (user.IsInRole("SuperUser") && user.FindFirstValue("IsEmployee") != "true")
        {
            if (!Guid.TryParse(user.FindFirstValue("CompanyId"), out var adminCompany) || !await db.Companies.AnyAsync(c => c.Id == adminCompany && c.IsActive))
            { context.Result = new ForbidResult(); return; }
            await next(); return;
        }
        if (!int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var employeeId)
            || !Guid.TryParse(user.FindFirstValue("CompanyId"), out var companyId))
        { context.Result = new ForbidResult(); return; }
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == employeeId && e.CompanyId == companyId && e.IsActive && e.Company!.IsActive);
        if (employee == null || area == "SuperUser") { context.Result = new ForbidResult(); return; }
        db.RestrictToCompanyId = companyId;
        var controller = context.RouteData.Values["controller"]?.ToString();
        if (controller == "Company") { context.Result = new ForbidResult(); return; }
        var action = context.RouteData.Values["action"]?.ToString() ?? "";
        if (controller == "Dashboard" && action == "Index")
        { context.Result = new RedirectToActionResult("ViewCalendar", "GenEmployee", new { area = "Administrator" }); return; }
        if (controller is "GenEmployee" or "LeaveHistory" or "InOutTimings") { await next(); return; }
        if (controller == "GenIncharge")
        {
            if (await db.Employees.AnyAsync(e => e.InchargeEmployeeId == employeeId && e.CompanyId == companyId && e.IsActive))
            { await next(); return; }
            // An explicitly assigned role can grant access before any team members
            // are assigned. Continue through the normal role permission checks;
            // the controller still limits employee records to this incharge's team.
        }
        var groupIds = db.GroupEmployees.Where(g => g.EmployeeId == employeeId && g.IsActive && g.Group!.IsActive).Select(g => g.GroupId);
        var roleIds = db.RoleAssignments.Where(r => r.CompanyId == companyId && r.IsActive && r.Role!.IsActive &&
            ((r.AssignmentType == "Individual" && r.EmployeeId == employeeId) ||
             (r.AssignmentType == "Group" && r.GroupId.HasValue && groupIds.Contains(r.GroupId.Value))))
            .Select(r => r.RoleId);
        var permissions = await db.RoleModules.Where(r => roleIds.Contains(r.RoleId) && r.Module!.IsActive && r.CanView &&
            (r.Module.Name == "Administrator" || (r.Module.Area == area && r.Module.Controller == controller))) .ToListAsync();
        var read = HttpMethods.IsGet(context.HttpContext.Request.Method) || action is "Search" or "LoadGroupEmployees";
        var allowed = permissions.Any(p => read || (action.StartsWith("Delete") ? p.CanDelete :
            action is "Create" || action.StartsWith("Add") ? p.CanCreate : p.CanEdit));
        context.Result = allowed ? null : new ForbidResult();
        if (allowed) await next();
    }
}

