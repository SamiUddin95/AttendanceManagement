using System.Security.Claims;
using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Web.Security;

public static class CurrentEmployee
{
    public static IQueryable<Employee> ForUser(this IQueryable<Employee> query, ClaimsPrincipal user)
    {
        _ = int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id);
        _ = Guid.TryParse(user.FindFirstValue("CompanyId"), out var companyId);
        return query.Where(e => user.FindFirstValue("IsEmployee") == "true" && e.Id == id && e.CompanyId == companyId && e.IsActive);
    }
}
