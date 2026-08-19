using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.LeavePolicy;
using AttendanceManagement.Web.ViewModels.LeaveFlexisChart;
using AttendanceManagement.Web.ViewModels.Calendar;
using AttendanceManagement.Web.ViewModels.ChangePassword;
using AttendanceManagement.Web.ViewModels.ApplyLeave;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using ApplyLeaveBalanceItem = AttendanceManagement.Web.ViewModels.ApplyLeave.LeaveBalanceItem;
using FlexisChartBalanceItem = AttendanceManagement.Web.ViewModels.LeaveFlexisChart.LeaveBalanceItem;
using CalendarEvent = AttendanceManagement.Web.ViewModels.Calendar.CalendarEvent;
using MonthItem = AttendanceManagement.Web.ViewModels.Calendar.MonthItem;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class GenEmployeeController : Controller
{
    private readonly AttendanceDbContext _context;

    public GenEmployeeController(AttendanceDbContext context)
    {
        _context = context;
    }

    public IActionResult LeaveHistory()
    {
        return RedirectToAction("Index", "LeaveHistory");
    }

    public IActionResult ReadMe()
    {
        ViewData["Title"] = "Read Me";
        return View();
    }

    public async Task<IActionResult> LeaveFlexisChart()
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new LeaveFlexisChartViewModel());
        }

        var viewModel = await BuildLeaveFlexisChartViewModelAsync(employee);
        ViewData["Title"] = "Leave/Flexis Chart";
        return View(viewModel);
    }

    public async Task<IActionResult> LeavePolicy()
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new EmployeeLeavePolicyViewModel());
        }

        var viewModel = await BuildLeavePolicyViewModelAsync(employee);
        ViewData["Title"] = "Leave Policy";
        return View(viewModel);
    }

    public IActionResult InOutTimings()
    {
        return RedirectToAction("Index", "InOutTimings");
    }

    public async Task<IActionResult> ViewCalendar(int? month, int? year)
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(CreateEmptyCalendarViewModel());
        }

        var viewModel = await BuildCalendarViewModelAsync(employee, month ?? DateTime.Now.Month, year ?? DateTime.Now.Year);
        ViewData["Title"] = "View Calendar";
        return View(viewModel);
    }

    private CalendarViewModel CreateEmptyCalendarViewModel()
    {
        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 2, 5).ToList();
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

        var monthOptions = availableMonths.Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Name, Selected = m.Value == DateTime.Now.Month }).ToList();
        var yearOptions = availableYears.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = y == DateTime.Now.Year }).ToList();

        return new CalendarViewModel
        {
            CurrentMonth = DateTime.Now.Month,
            CurrentYear = DateTime.Now.Year,
            AvailableYears = availableYears,
            AvailableMonths = availableMonths,
            MonthOptions = new SelectList(monthOptions, "Value", "Text"),
            YearOptions = new SelectList(yearOptions, "Value", "Text")
        };
    }

    public IActionResult ChangePassword()
    {
        ViewData["Title"] = "Change Password";
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel viewModel)
    {
        ViewData["Title"] = "Change Password";

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(viewModel);
        }

        if (string.IsNullOrWhiteSpace(employee.Password) || !BCrypt.Net.BCrypt.Verify(viewModel.CurrentPassword, employee.Password))
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
            return View(viewModel);
        }

        employee.Password = BCrypt.Net.BCrypt.HashPassword(viewModel.NewPassword);
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Password changed successfully.";
        return RedirectToAction(nameof(ChangePassword));
    }

    public async Task<IActionResult> ApplyForLeave()
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new ApplyLeaveViewModel());
        }

        var viewModel = await BuildApplyLeaveViewModelAsync(employee);
        ViewData["Title"] = "Apply for Leave";
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyForLeave(ApplyLeaveViewModel viewModel)
    {
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(viewModel);
        }

        // Re-populate leave types and balances for validation
        var fullViewModel = await BuildApplyLeaveViewModelAsync(employee);
        viewModel.LeaveTypes = fullViewModel.LeaveTypes;
        viewModel.LeaveBalances = fullViewModel.LeaveBalances;
        viewModel.LeaveTypeOptions = fullViewModel.LeaveTypeOptions;
        viewModel.EmployeeId = fullViewModel.EmployeeId;
        viewModel.EmployeeName = fullViewModel.EmployeeName;
        viewModel.EmployeeNo = fullViewModel.EmployeeNo;

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        // Validate dates
        if (viewModel.FromDate < DateTime.Today)
        {
            ModelState.AddModelError("FromDate", "From date cannot be in the past.");
            return View(viewModel);
        }

        if (viewModel.ToDate < viewModel.FromDate)
        {
            ModelState.AddModelError("ToDate", "To date cannot be before from date.");
            return View(viewModel);
        }

        // Validate leave balance
        var selectedBalance = fullViewModel.LeaveBalances.FirstOrDefault(b => b.LeaveType == viewModel.SelectedLeaveType);
        if (selectedBalance != null && viewModel.TotalDays > selectedBalance.RemainingDays)
        {
            ModelState.AddModelError("FromDate", $"Insufficient {viewModel.SelectedLeaveType} balance. You have {selectedBalance.RemainingDays} days remaining.");
            return View(viewModel);
        }

        // Generate leave number
        var leaveCount = await _context.LeaveApplications.CountAsync(la => la.EmployeeId == employee.Id);
        var leaveNumber = $"LV-{employee.EmployeeNo}-{DateTime.Now:yyyyMMdd}-{leaveCount + 1:D3}";

        // Create leave application
        var leaveApplication = new LeaveApplication
        {
            CompanyId = employee.CompanyId,
            EmployeeId = employee.Id,
            LeaveType = viewModel.SelectedLeaveType,
            StartDate = viewModel.FromDate,
            EndDate = viewModel.ToDate,
            TotalDays = viewModel.TotalDays,
            Reason = viewModel.Reason,
            Status = "Pending",
            LeaveNumber = leaveNumber,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.LeaveApplications.Add(leaveApplication);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Leave application submitted successfully. Leave Number: {leaveNumber}";
        return RedirectToAction(nameof(ApplyForLeave));
    }

    private async Task<ApplyLeaveViewModel> BuildApplyLeaveViewModelAsync(Employee employee)
    {
        var leave = employee.Leave;
        var leaveApplications = await _context.LeaveApplications
            .Where(la => la.EmployeeId == employee.Id && la.Status == "Approved")
            .ToListAsync();

        var balances = new List<ApplyLeaveBalanceItem>();
        var leaveTypes = new List<LeaveTypeOption>();
        var selectListItems = new List<SelectListItem>();

        if (leave?.LeaveDetails != null)
        {
            foreach (var detail in leave.LeaveDetails)
            {
                var usedDays = leaveApplications
                    .Where(la => la.LeaveType == detail.LeaveType)
                    .Sum(la => la.TotalDays);

                var remainingDays = Math.Max(0, detail.NoOfDays - usedDays);

                balances.Add(new ApplyLeaveBalanceItem
                {
                    LeaveType = detail.LeaveType,
                    EntitledDays = detail.NoOfDays,
                    UsedDays = usedDays,
                    RemainingDays = remainingDays,
                    CarryForward = detail.CarryForward
                });

                leaveTypes.Add(new LeaveTypeOption
                {
                    LeaveType = detail.LeaveType,
                    AvailableDays = remainingDays,
                    CarryForward = detail.CarryForward
                });

                selectListItems.Add(new SelectListItem
                {
                    Value = detail.LeaveType,
                    Text = $"{detail.LeaveType} ({remainingDays} days available)"
                });
            }
        }

        return new ApplyLeaveViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            LeaveTypes = leaveTypes,
            LeaveBalances = balances,
            LeaveTypeOptions = new SelectList(selectListItems, "Value", "Text"),
            FromDate = DateTime.Today.AddDays(1),
            ToDate = DateTime.Today.AddDays(1)
        };
    }

    private async Task<Employee?> GetLoggedInEmployeeAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        var employee = await _context.Employees
            .Include(e => e.Leave)
                .ThenInclude(l => l.LeaveDetails)
            .Include(e => e.Leave)
                .ThenInclude(l => l.LeavePolicy)
            .FirstOrDefaultAsync(e => e.EmployeeNo == userName || e.Email == userName);

        return employee;
    }

    private async Task<EmployeeLeavePolicyViewModel> BuildLeavePolicyViewModelAsync(Employee employee)
    {
        var leave = employee.Leave;
        var leavePolicy = leave?.LeavePolicy;

        var details = leave?.LeaveDetails.Select(ld => new LeaveDetailViewModel
        {
            LeaveType = ld.LeaveType,
            NoOfDays = ld.NoOfDays,
            CarryForward = ld.CarryForward
        }).ToList() ?? new List<LeaveDetailViewModel>();

        return new EmployeeLeavePolicyViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            LeavePolicyName = leavePolicy?.Name ?? "Not Assigned",
            LeavePolicyStatus = leavePolicy?.Status ?? "N/A",
            LeaveName = leave?.Name ?? "Not Assigned",
            LeaveDescription = leave?.Description ?? string.Empty,
            LeaveDetails = details
        };
    }

    private async Task<LeaveFlexisChartViewModel> BuildLeaveFlexisChartViewModelAsync(Employee employee)
    {
        var leave = employee.Leave;
        var leaveApplications = await _context.LeaveApplications
            .Where(la => la.EmployeeId == employee.Id && la.Status == "Approved")
            .ToListAsync();

        var balances = new List<FlexisChartBalanceItem>();

        if (leave?.LeaveDetails != null)
        {
            foreach (var detail in leave.LeaveDetails)
            {
                var usedDays = leaveApplications
                    .Where(la => la.LeaveType == detail.LeaveType)
                    .Sum(la => la.TotalDays);

                balances.Add(new FlexisChartBalanceItem
                {
                    LeaveType = detail.LeaveType,
                    EntitledDays = detail.NoOfDays,
                    UsedDays = usedDays,
                    RemainingDays = Math.Max(0, detail.NoOfDays - usedDays),
                    CarryForward = detail.CarryForward
                });
            }
        }

        return new LeaveFlexisChartViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            LeaveBalances = balances,
            TotalEntitledDays = balances.Sum(b => b.EntitledDays),
            TotalUsedDays = balances.Sum(b => b.UsedDays),
            TotalRemainingDays = balances.Sum(b => b.RemainingDays)
        };
    }

    private async Task<CalendarViewModel> BuildCalendarViewModelAsync(Employee employee, int month, int year)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var events = new List<CalendarEvent>();

        // Get approved leave applications for this month
        var leaveApplications = await _context.LeaveApplications
            .Where(la => la.EmployeeId == employee.Id &&
                         la.Status == "Approved" &&
                         la.StartDate <= endDate &&
                         la.EndDate >= startDate)
            .ToListAsync();

        foreach (var la in leaveApplications)
        {
            var leaveStart = la.StartDate > startDate ? la.StartDate : startDate;
            var leaveEnd = la.EndDate < endDate ? la.EndDate : endDate;

            for (var date = leaveStart; date <= leaveEnd; date = date.AddDays(1))
            {
                events.Add(new CalendarEvent
                {
                    Date = date,
                    Title = $"{la.LeaveType} Leave",
                    Type = "Leave",
                    Color = "#fd7e14",
                    Description = la.Reason
                });
            }
        }

        // Get MarkDays (holidays) for this month
        var markDays = await _context.MarkDays
            .Where(md => md.CompanyId == employee.CompanyId &&
                         md.FromDate <= endDate &&
                         md.ToDate >= startDate)
            .ToListAsync();

        foreach (var md in markDays)
        {
            var holidayStart = md.FromDate > startDate ? md.FromDate : startDate;
            var holidayEnd = md.ToDate < endDate ? md.ToDate : endDate;

            for (var date = holidayStart; date <= holidayEnd; date = date.AddDays(1))
            {
                var holidayTitle = "Holiday";
                if (md.IsGazettedHoliday) holidayTitle = "Gazetted Holiday";
                else if (md.IsProvincialHoliday) holidayTitle = "Provincial Holiday";
                else if (md.IsEid) holidayTitle = "Eid Holiday";
                else if (md.IsStrike) holidayTitle = "Strike Day";
                else if (md.IsOn) holidayTitle = "Working Day";
                else if (md.IsOff) holidayTitle = "Off Day";

                events.Add(new CalendarEvent
                {
                    Date = date,
                    Title = holidayTitle,
                    Type = "Holiday",
                    Color = "#198754",
                    Description = holidayTitle
                });
            }
        }

        // Add employee birthday if in this month
        if (employee.DateOfBirth.HasValue && employee.DateOfBirth.Value.Month == month)
        {
            var birthday = new DateTime(year, month, employee.DateOfBirth.Value.Day);
            if (birthday >= startDate && birthday <= endDate)
            {
                events.Add(new CalendarEvent
                {
                    Date = birthday,
                    Title = "Your Birthday",
                    Type = "Birthday",
                    Color = "#e83e8c",
                    Description = "Happy Birthday!"
                });
            }
        }

        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 2, 5).ToList();
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

        var monthOptions = availableMonths.Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Name, Selected = m.Value == month }).ToList();
        var yearOptions = availableYears.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = y == year }).ToList();

        return new CalendarViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            CurrentMonth = month,
            CurrentYear = year,
            Events = events.OrderBy(e => e.Date).ToList(),
            AvailableYears = availableYears,
            AvailableMonths = availableMonths,
            MonthOptions = new SelectList(monthOptions, "Value", "Text"),
            YearOptions = new SelectList(yearOptions, "Value", "Text")
        };
    }
}
