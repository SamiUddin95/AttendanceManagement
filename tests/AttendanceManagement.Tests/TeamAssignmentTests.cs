using System.Security.Claims;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.Areas.Administrator.Controllers;
using AttendanceManagement.Web.ViewModels.Employees;
using AttendanceManagement.Web.ViewModels.GenIncharge;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class TeamAssignmentTests
{
    [Theory]
    [InlineData("valid", true)]
    [InlineData("self", false)]
    [InlineData("foreign", false)]
    [InlineData("inactive", false)]
    [InlineData("cycle", false)]
    [InlineData("reassign", false)]
    [InlineData("confirmed", true)]
    [InlineData("missing", false)]
    public async Task BulkAssignmentIsAtomicAndAppearsInInchargeTeam(string scenario, bool expected)
    {
        await using var db = new AttendanceDbContext(new DbContextOptionsBuilder<AttendanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var company = new Company { Name = "Hospital" };
        var incharge = new Employee { Id = 100, Company = company, Name = "Supervisor", EmployeeNo = "23455" };
        var first = new Employee { Id = 101, Company = company, Name = "Nurse One", EmployeeNo = "ONE" };
        var second = new Employee { Id = 102, Company = company, Name = "Nurse Two", EmployeeNo = "TWO" };
        if (scenario == "foreign") second.Company = new Company { Name = "Other Hospital" };
        if (scenario == "inactive") second.IsActive = false;
        if (scenario == "cycle") incharge.InchargeEmployeeId = second.Id;
        if (scenario is "reassign" or "confirmed") second.InchargeEmployeeId = first.Id;
        db.AddRange(incharge, first, second);
        await db.SaveChangesAsync();
        var http = new DefaultHttpContext();
        http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, incharge.Id.ToString()), new Claim(ClaimTypes.Name, "23455"),
            new Claim("CompanyId", company.Id.ToString()), new Claim("IsEmployee", "true") }, "Test"));
        var controller = new EmployeeController(db) {
            ControllerContext = new ControllerContext { HttpContext = http },
            TempData = new TempDataDictionary(http, new MemoryTempData())
        };
        var selection = new List<int> { first.Id, scenario == "self" ? incharge.Id : scenario == "missing" ? 999 : second.Id };
        var result = await controller.AssignTeam(new AssignTeamViewModel {
            InchargeId = incharge.Id, EmployeeIds = selection, AllowReassignment = scenario == "confirmed" });
        if (expected)
        {
            Assert.IsType<RedirectToActionResult>(result);
            db.ChangeTracker.Clear();
            var teamController = new GenInchargeController(db) { ControllerContext = new ControllerContext { HttpContext = http } };
            var view = Assert.IsType<ViewResult>(await teamController.TeamMembers());
            var team = Assert.IsType<TeamMembersViewModel>(view.Model);
            Assert.Equal(2, team.Members.Count);
        }
        else
        {
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            db.ChangeTracker.Clear();
            Assert.Null((await db.Employees.FindAsync(first.Id))!.InchargeEmployeeId);
        }
    }

    private class MemoryTempData : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
        public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
    }
}
