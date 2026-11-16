using AttendanceManagement.Web.Security;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.InOutTimings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class InOutTimingsController : Controller
{
    private readonly AttendanceDbContext _context;

    public InOutTimingsController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(CreateEmptyViewModel());
        }

        var viewModel = await BuildViewModelAsync(employee, DateTime.Now.Month, DateTime.Now.Year, true, false);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(InOutTimingsViewModel viewModel)
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View("Index", CreateEmptyViewModel());
        }

        var updatedViewModel = await BuildViewModelAsync(
            employee,
            viewModel.SelectedMonth,
            viewModel.SelectedYear,
            viewModel.IsPayrollWise,
            viewModel.IsMonthWise
        );

        return View("Index", updatedViewModel);
    }

    private InOutTimingsViewModel CreateEmptyViewModel()
    {
        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 5, 7).ToList();
        var availableMonths = new List<MonthItem>
        {
            new() { Value = 1, Name = "January" },
            new() { Value = 2, Name = "February" },
            new() { Value = 3, Name = "March" },
            new() { Value = 4, Name = "April" },
            new() { Value = 5, Name = "May" },
            new() { Value = 6, Name = "June" },
            new() { Value = 7, Name = "July" },
            new() { Value = 8, Name = "August" },
            new() { Value = 9, Name = "September" },
            new() { Value = 10, Name = "October" },
            new() { Value = 11, Name = "November" },
            new() { Value = 12, Name = "December" }
        };

        return new InOutTimingsViewModel
        {
            SelectedMonth = DateTime.Now.Month,
            SelectedYear = DateTime.Now.Year,
            IsPayrollWise = true,
            IsMonthWise = false,
            AvailableYears = availableYears,
            AvailableMonths = availableMonths
        };
    }

    private async Task<Employee?> GetLoggedInEmployeeAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var employee = await _context.Employees
            .Include(e => e.Company)
            .ForUser(User).FirstOrDefaultAsync();

        return employee;
    }

    private async Task<InOutTimingsViewModel> BuildViewModelAsync(
        Employee employee,
        int month,
        int year,
        bool isPayrollWise,
        bool isMonthWise)
    {
        // Populate available years (current year - 5 to current year + 1)
        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 5, 7).ToList();

        // Populate available months
        var availableMonths = new List<MonthItem>
        {
            new() { Value = 1, Name = "January" },
            new() { Value = 2, Name = "February" },
            new() { Value = 3, Name = "March" },
            new() { Value = 4, Name = "April" },
            new() { Value = 5, Name = "May" },
            new() { Value = 6, Name = "June" },
            new() { Value = 7, Name = "July" },
            new() { Value = 8, Name = "August" },
            new() { Value = 9, Name = "September" },
            new() { Value = 10, Name = "October" },
            new() { Value = 11, Name = "November" },
            new() { Value = 12, Name = "December" }
        };

        // Calculate date range for the selected month
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

        // Get attendance records for the employee in the selected month
        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employee.Id &&
                        a.PunchDateTime >= startDate.AddDays(-1) &&
                        a.PunchDateTime < endDate.Date.AddDays(2))
            .OrderByDescending(a => a.PunchDateTime)
            .ToListAsync();

        // Process attendances to create In/Out pairs
        var inOutItems = ProcessAttendances(attendances, startDate, endDate, isPayrollWise);

        return new InOutTimingsViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            SelectedMonth = month,
            SelectedYear = year,
            IsPayrollWise = isPayrollWise,
            IsMonthWise = isMonthWise,
            InOutTimingsItems = inOutItems,
            AvailableYears = availableYears,
            AvailableMonths = availableMonths
        };
    }

    private List<InOutTimingsItem> ProcessAttendances(List<Attendance> attendances, DateTime startDate, DateTime endDate, bool isPayrollWise)
    {
        var rows = AttendanceManagement.Web.Services.PunchPairing.Build(attendances)
            .Where(r => r.Date >= startDate.Date && r.Date <= endDate.Date)
            .OrderByDescending(r => r.Date)
            .ToList();
        for (var index = 0; index < rows.Count; index++) rows[index].SrNo = index + 1;
        return rows;
    }
}
