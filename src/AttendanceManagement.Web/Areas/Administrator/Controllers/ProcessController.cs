using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator"), Authorize]
public class ProcessController(AttendanceDbContext db) : Controller
{
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(2_000_000)]
    public async Task<IActionResult> Import(IFormFile? file)
    {
        if (file == null || file.Length == 0 || file.Length > 1_000_000)
        { ModelState.AddModelError("file", "Choose a CSV file of up to 1 MB."); return View("Index"); }
        List<DevicePunch> punches;
        try { using var reader = new StreamReader(file.OpenReadStream()); punches = DeviceCsv.Parse(reader); }
        catch (Exception ex) when (ex is FormatException or MalformedLineException)
        { ModelState.AddModelError("file", ex.Message); return View("Index"); }
        if (!Guid.TryParse(User.FindFirst("CompanyId")?.Value, out var companyId)) return Forbid();
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var numbers = punches.Select(p => p.EmployeeNo).Distinct().ToList();
        var employees = await db.Employees.Where(e => e.CompanyId == companyId && numbers.Contains(e.EmployeeNo)).ToListAsync();
        if (numbers.Any(n => employees.Count(e => e.EmployeeNo == n) != 1))
        { ModelState.AddModelError("file", "Every employee number must identify exactly one employee in your company. No records were imported."); return View("Index"); }
        var from = punches.Min(p => p.Timestamp); var to = punches.Max(p => p.Timestamp);
        var ids = employees.Select(e => e.Id).ToList();
        var existing = await db.Attendances.Where(a => a.CompanyId == companyId && ids.Contains(a.EmployeeId) && a.PunchDateTime >= from && a.PunchDateTime <= to)
            .Select(a => new { a.EmployeeId, a.PunchDateTime, a.PunchType }).ToListAsync();
        var keys = existing.Select(a => (a.EmployeeId, a.PunchDateTime, a.PunchType)).ToHashSet();
        var imported = 0;
        foreach (var punch in punches)
        {
            var employee = employees.Single(e => e.EmployeeNo == punch.EmployeeNo);
            if (!keys.Add((employee.Id, punch.Timestamp, punch.PunchType))) continue;
            db.Attendances.Add(new() { CompanyId = companyId, EmployeeId = employee.Id, PunchDateTime = punch.Timestamp,
                PunchType = punch.PunchType, DeviceId = punch.DeviceId, Location = "Device CSV import" });
            imported++;
        }
        await db.SaveChangesAsync(); await transaction.CommitAsync();
        TempData["SuccessMessage"] = $"Imported {imported} punches. Skipped {punches.Count - imported} duplicates.";
        return RedirectToAction(nameof(Index));
    }
}
