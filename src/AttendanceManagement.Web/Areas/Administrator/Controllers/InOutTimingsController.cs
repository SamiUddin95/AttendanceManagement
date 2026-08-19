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
            .FirstOrDefaultAsync(e => e.EmployeeNo == userName || e.Email == userName);

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
                        a.PunchDateTime >= startDate &&
                        a.PunchDateTime <= endDate)
            .OrderBy(a => a.PunchDateTime)
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

    private List<InOutTimingsItem> ProcessAttendances(
        List<Attendance> attendances,
        DateTime startDate,
        DateTime endDate,
        bool isPayrollWise)
    {
        var items = new List<InOutTimingsItem>();
        var srNo = 1;

        // Group attendances by date
        var groupedByDate = attendances
            .GroupBy(a => a.PunchDateTime.Date)
            .OrderBy(g => g.Key)
            .ToList();

        foreach (var group in groupedByDate)
        {
            var dateAttendances = group.OrderBy(a => a.PunchDateTime).ToList();
            var timeIn = dateAttendances.FirstOrDefault(a => a.PunchType == "In")?.PunchDateTime.TimeOfDay;
            var timeOut = dateAttendances.LastOrDefault(a => a.PunchType == "Out")?.PunchDateTime.TimeOfDay;

            // Calculate extra hours if both In and Out exist
            TimeSpan? extraHours = null;
            if (timeIn.HasValue && timeOut.HasValue)
            {
                var totalHours = timeOut.Value - timeIn.Value;
                // Assuming standard 8 hours work day
                var standardHours = TimeSpan.FromHours(8);
                if (totalHours > standardHours)
                {
                    extraHours = totalHours - standardHours;
                }
            }

            var status = DetermineStatus(timeIn, timeOut);

            items.Add(new InOutTimingsItem
            {
                SrNo = srNo++,
                Date = group.Key,
                TimeIn = timeIn,
                TimeOut = timeOut,
                ExtraHours = extraHours,
                Status = status
            });
        }

        // If payroll wise, we might need to group by payroll period (e.g., 1-15, 16-end of month)
        if (isPayrollWise)
        {
            // For payroll wise, we can add a header row or group indicator
            // This is a simplified version - in real payroll systems, you'd have payroll periods defined
        }

        return items;
    }

    private string DetermineStatus(TimeSpan? timeIn, TimeSpan? timeOut)
    {
        if (!timeIn.HasValue && !timeOut.HasValue)
            return "Absent";

        if (timeIn.HasValue && !timeOut.HasValue)
            return "Half Day (No Out)";

        if (!timeIn.HasValue && timeOut.HasValue)
            return "Half Day (No In)";

        var totalHours = timeOut.Value - timeIn.Value;
        if (totalHours < TimeSpan.FromHours(4))
            return "Half Day";

        return "Present";
    }
}
