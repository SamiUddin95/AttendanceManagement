using System.Security.Claims;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.Areas.Administrator.Controllers;
using AttendanceManagement.Web.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class ModuleAccessFilterTests
{
    [Theory]
    [InlineData("Individual", true, true, true, "GET", true)]
    [InlineData("Group", true, true, true, "GET", true)]
    [InlineData("Individual", false, true, true, "GET", false)]
    [InlineData("Individual", true, false, true, "GET", false)]
    [InlineData("Individual", true, true, false, "GET", false)]
    [InlineData("Individual", true, true, true, "POST", false)]
    [InlineData("None", true, true, true, "GET", false)]
    public async Task InchargeWithoutTeamHonorsRolePermissions(string assignmentType, bool canView,
        bool roleActive, bool assignmentActive, string method, bool expectedAccess)
    {
        await using var db = CreateDatabase();
        var company = new Company { Name = "Test Hospital" };
        var employee = new Employee { Id = 100, EmployeeNo = "23455", Name = "Test Employee", Company = company };
        var role = new Role { Name = "Incharge", IsActive = roleActive };
        var module = new Module { Id = 100, Name = "GenIncharge", Area = "Administrator", Controller = "GenIncharge", IsActive = true };
        db.AddRange(employee, role, module);
        db.RoleModules.Add(new RoleModule { Role = role, Module = module, CanView = canView, CanEdit = false });
        if (assignmentType != "None")
        {
            var assignment = new RoleAssignment { Company = company, Role = role,
                AssignmentType = assignmentType, IsActive = assignmentActive };
            if (assignmentType == "Individual") assignment.Employee = employee;
            else
            {
                var group = new Group { Name = "Test Team", Code = "TEST", Company = company };
                db.GroupEmployees.Add(new GroupEmployee { Group = group, Employee = employee });
                assignment.Group = group;
            }
            db.RoleAssignments.Add(assignment);
        }
        await db.SaveChangesAsync();
        var actionContext = CreateContext(employee.Id, company.Id, method);
        var executing = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());
        var invoked = false;
        await new ModuleAccessFilter(db).OnActionExecutionAsync(executing, () =>
        {
            invoked = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object()));
        });
        Assert.Equal(expectedAccess, invoked);
        if (!expectedAccess) Assert.IsType<ForbidResult>(executing.Result);
    }

    [Fact]
    public async Task GrantedModuleDoesNotExposeAnotherInchargesEmployeeCalendar()
    {
        await using var db = CreateDatabase();
        var company = new Company { Name = "Test Hospital" };
        var employee = new Employee { Id = 100, EmployeeNo = "23455", Name = "Test Employee", Company = company };
        var unrelated = new Employee { Id = 101, EmployeeNo = "OTHER", Name = "Other Employee", Company = company };
        db.AddRange(employee, unrelated);
        await db.SaveChangesAsync();
        db.RestrictToCompanyId = company.Id;
        var controller = new GenInchargeController(db)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateContext(employee.Id, company.Id, "GET").HttpContext }
        };
        Assert.IsType<ForbidResult>(await controller.ViewCalendar(10, 2026, unrelated.Id));
        Assert.IsType<ViewResult>(await controller.TeamMembers());
    }

    private static AttendanceDbContext CreateDatabase() => new(new DbContextOptionsBuilder<AttendanceDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ActionContext CreateContext(int employeeId, Guid companyId, string method)
    {
        var http = new DefaultHttpContext();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employeeId.ToString()), new Claim("CompanyId", companyId.ToString()),
            new Claim(ClaimTypes.Name, "23455"),
            new Claim("IsEmployee", "true"), new Claim(ClaimTypes.Role, "Employee")
        }, "Test"));
        http.Request.Method = method;
        var route = new RouteData();
        route.Values["area"] = "Administrator";
        route.Values["controller"] = "GenIncharge";
        route.Values["action"] = method == "GET" ? "TeamMembers" : "ApproveLeave";
        return new ActionContext(http, route, new ActionDescriptor());
    }
}
