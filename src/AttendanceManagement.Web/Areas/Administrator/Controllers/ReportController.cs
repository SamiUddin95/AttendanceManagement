using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.Services;
using AttendanceManagement.Web.ViewModels.InOutTimings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator"), Authorize]
public class ReportController(AttendanceDbContext db) : Controller
{
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var start = (from ?? DateTime.Today.AddDays(-7)).Date;
        var end = (to ?? DateTime.Today).Date;
        if (end < start || (end - start).TotalDays > 92 || start.Year < 2000 || end.Year > 2100)
            return BadRequest("Choose a reporting period of at most 93 days between 2000 and 2100.");
        Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var company);
        var punches = await db.Attendances.AsNoTracking().Include(a => a.Employee)
            .Where(a => a.CompanyId == company && a.PunchDateTime >= start.AddDays(-1) && a.PunchDateTime < end.AddDays(2))
            .OrderBy(a => a.EmployeeId).ThenBy(a => a.PunchDateTime).ToListAsync();
        var rows = punches.GroupBy(a => a.EmployeeId).SelectMany(g => PunchPairing.Build(g)
            .Where(r => r.Date >= start && r.Date <= end)
            .Select(r => new RecordedAttendanceRow(g.First().Employee!.EmployeeNo, g.First().Employee!.Name, r))).ToList();
        ViewBag.From = start.ToString("yyyy-MM-dd"); ViewBag.To = end.ToString("yyyy-MM-dd");
        return View(rows);
    }
}
public record RecordedAttendanceRow(string EmployeeNo, string EmployeeName, InOutTimingsItem Timing);
