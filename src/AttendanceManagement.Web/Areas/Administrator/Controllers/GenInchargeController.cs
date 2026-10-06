using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Calendar;
using AttendanceManagement.Web.ViewModels.GenIncharge;
using AttendanceManagement.Web.ViewModels.InOutTimings;
using AttendanceManagement.Web.ViewModels.LeaveFlexisChart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using EmployeeLeaveBalanceItem = AttendanceManagement.Web.ViewModels.GenIncharge.EmployeeLeaveBalance;
using FlexisChartBalanceItem = AttendanceManagement.Web.ViewModels.LeaveFlexisChart.LeaveBalanceItem;
using CalendarEvent = AttendanceManagement.Web.ViewModels.Calendar.CalendarEvent;
using MonthItem = AttendanceManagement.Web.ViewModels.Calendar.MonthItem;
using InChargeMonthItem = AttendanceManagement.Web.ViewModels.InOutTimings.MonthItem;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class GenInchargeController : Controller
{
    private readonly AttendanceDbContext _context;

    public GenInchargeController(AttendanceDbContext context)
    {
        _context = context;
    }

    // ── Calendar ────────────────────────────────────────────────────────────

    public async Task<IActionResult> ViewCalendar(int? month, int? year, int? employeeId, string? viewMode = "team")
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(CreateEmptyCalendarViewModel());
        }

        var teamMembers = await _context.Employees
            .Where(e => e.InchargeEmployeeId == incharge.Id)
            .OrderBy(e => e.Name)
            .Select(e => new { e.Id, e.Name, e.EmployeeNo })
            .ToListAsync();

        var selectItems = teamMembers
            .Select(e => new SelectListItem($"{e.Name} ({e.EmployeeNo})", e.Id.ToString(), e.Id == employeeId))
            .ToList();
        selectItems.Insert(0, new SelectListItem("All Team Members", "", !employeeId.HasValue));
        ViewBag.TeamEmployeeOptions = selectItems;
        ViewBag.SelectedEmployeeId = employeeId;
        ViewBag.ViewMode = viewMode?.ToLowerInvariant() ?? "team";

        var monthValue = month ?? DateTime.Now.Month;
        var yearValue = year ?? DateTime.Now.Year;

        // if employeeId selected, it overrides viewMode; otherwise pass viewMode
        var vm = await BuildCalendarViewModelAsync(incharge, monthValue, yearValue, employeeId, viewMode);
        ViewData["Title"] = employeeId.HasValue
            ? $"Calendar — {teamMembers.FirstOrDefault(t => t.Id == employeeId)?.Name}"
            : viewMode == "myself" ? "My Calendar" : "Team Calendar";
        return View(vm);
    }

    // ── Team Members ─────────────────────────────────────────────────────────

    public async Task<IActionResult> TeamMembers()
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new TeamMembersViewModel());
        }

        var members = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Category)
            .Include(e => e.Shift)
            .Where(e => e.InchargeEmployeeId == incharge.Id)
            .OrderBy(e => e.Name)
            .ToListAsync();

        var vm = new TeamMembersViewModel
        {
            InchargeId = incharge.Id,
            InchargeName = incharge.Name,
            InchargeNo = incharge.EmployeeNo,
            Members = members.Select(e => new TeamMemberItem
            {
                Id = e.Id,
                EmployeeNo = e.EmployeeNo,
                Name = e.Name,
                DepartmentName = e.Department?.Name,
                DesignationName = e.Designation?.Name,
                CategoryName = e.Category?.Name,
                ShiftName = e.Shift?.Name,
                EmployeeType = e.EmployeeType,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive,
                Email = e.Email,
                Phone = e.Phone ?? e.Cell
            }).ToList()
        };

        ViewData["Title"] = "Team Members";
        return View(vm);
    }

    // ── Employee View ─────────────────────────────────────────────────────────

    public async Task<IActionResult> EmployeeView(int? employeeId)
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new EmployeeViewViewModel());
        }

        var teamMembers = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Category)
            .Include(e => e.Shift)
            .Include(e => e.Leave)
                .ThenInclude(l => l!.LeaveDetails)
            .Where(e => e.InchargeEmployeeId == incharge.Id)
            .OrderBy(e => e.Name)
            .ToListAsync();

        var selectItems = teamMembers
            .Select(e => new SelectListItem($"{e.Name} ({e.EmployeeNo})", e.Id.ToString(), e.Id == employeeId))
            .ToList();

        var vm = new EmployeeViewViewModel
        {
            SelectedEmployeeId = employeeId,
            EmployeeOptions = new SelectList(selectItems, "Value", "Text")
        };

        if (employeeId.HasValue)
        {
            var emp = teamMembers.FirstOrDefault(e => e.Id == employeeId.Value);
            if (emp == null || emp.InchargeEmployeeId != incharge.Id)
            {
                TempData["ErrorMessage"] = "Employee not found in your team.";
                return View(vm);
            }

            vm.Profile = new EmployeeProfileDetail
            {
                Id = emp.Id,
                EmployeeNo = emp.EmployeeNo,
                Name = emp.Name,
                DepartmentName = emp.Department?.Name,
                DesignationName = emp.Designation?.Name,
                CategoryName = emp.Category?.Name,
                ShiftName = emp.Shift?.Name,
                EmployeeType = emp.EmployeeType,
                JoiningDate = emp.JoiningDate,
                DateOfBirth = emp.DateOfBirth,
                Gender = emp.Gender,
                BloodGroup = emp.BloodGroup,
                Email = emp.Email,
                Phone = emp.Phone,
                Cell = emp.Cell,
                Address = emp.Address,
                CNIC = emp.CNIC,
                IsActive = emp.IsActive,
                OverTimeEntitled = emp.OverTimeEntitled,
                LeaveName = emp.Leave?.Name
            };

            var approvedLeaves = await _context.LeaveApplications
                .Where(la => la.EmployeeId == emp.Id && la.Status == "Approved")
                .ToListAsync();

            if (emp.Leave?.LeaveDetails != null)
            {
                foreach (var detail in emp.Leave.LeaveDetails)
                {
                    var used = approvedLeaves
                        .Where(la => la.LeaveType == detail.LeaveType)
                        .Sum(la => la.TotalDays);

                    vm.LeaveBalances.Add(new EmployeeLeaveBalanceItem
                    {
                        LeaveType = detail.LeaveType,
                        EntitledDays = detail.NoOfDays,
                        UsedDays = used,
                        RemainingDays = Math.Max(0, detail.NoOfDays - used),
                        CarryForward = detail.CarryForward
                    });
                }
            }

            vm.LeaveHistory = await _context.LeaveApplications
                .Where(la => la.EmployeeId == emp.Id)
                .OrderByDescending(la => la.CreatedAtUtc)
                .Select(la => new EmployeeLeaveHistoryItem
                {
                    Id = la.Id,
                    LeaveNumber = la.LeaveNumber,
                    LeaveType = la.LeaveType,
                    StartDate = la.StartDate,
                    EndDate = la.EndDate,
                    TotalDays = la.TotalDays,
                    Reason = la.Reason,
                    Status = la.Status,
                    InchargeApprovalStatus = la.InchargeApprovalStatus,
                    CreatedAtUtc = la.CreatedAtUtc
                })
                .ToListAsync();
        }

        ViewData["Title"] = "Employee View";
        return View(vm);
    }

    // ── Team Leave Requests ───────────────────────────────────────────────────

    public async Task<IActionResult> TeamLeaveRequests()
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new TeamLeaveRequestsViewModel());
        }

        var teamIds = await _context.Employees
            .Where(e => e.InchargeEmployeeId == incharge.Id)
            .Select(e => e.Id)
            .ToListAsync();

        var requests = await _context.LeaveApplications
            .Include(la => la.Employee)
                .ThenInclude(e => e!.Department)
            .Where(la => teamIds.Contains(la.EmployeeId))
            .OrderByDescending(la => la.CreatedAtUtc)
            .ToListAsync();

        var vm = new TeamLeaveRequestsViewModel
        {
            InchargeId = incharge.Id,
            InchargeName = incharge.Name,
            Requests = requests.Select(la => new TeamLeaveRequestItem
            {
                Id = la.Id,
                LeaveNumber = la.LeaveNumber,
                EmployeeId = la.EmployeeId,
                EmployeeName = la.Employee?.Name ?? string.Empty,
                EmployeeNo = la.Employee?.EmployeeNo ?? string.Empty,
                DepartmentName = la.Employee?.Department?.Name,
                LeaveType = la.LeaveType,
                StartDate = la.StartDate,
                EndDate = la.EndDate,
                TotalDays = la.TotalDays,
                Reason = la.Reason,
                Status = la.Status,
                InchargeApprovalStatus = la.InchargeApprovalStatus,
                InchargeApprovedAt = la.InchargeApprovedAt,
                CreatedAtUtc = la.CreatedAtUtc
            }).ToList()
        };

        ViewData["Title"] = "Team Leave Requests";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null) return Forbid();

        var application = await _context.LeaveApplications
            .FirstOrDefaultAsync(la => la.Id == id);

        if (application == null) return NotFound();

        // Verify this employee belongs to the incharge's team
        var isTeamMember = await _context.Employees
            .AnyAsync(e => e.Id == application.EmployeeId && e.InchargeEmployeeId == incharge.Id);

        if (!isTeamMember) return Forbid();

        application.InchargeId = incharge.Id;
        application.InchargeApprovalStatus = "Approved";
        application.InchargeApprovedAt = DateTime.UtcNow;
        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Leave request {application.LeaveNumber} approved.";
        return RedirectToAction(nameof(TeamLeaveRequests));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectLeave(int id)
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null) return Forbid();

        var application = await _context.LeaveApplications
            .FirstOrDefaultAsync(la => la.Id == id);

        if (application == null) return NotFound();

        var isTeamMember = await _context.Employees
            .AnyAsync(e => e.Id == application.EmployeeId && e.InchargeEmployeeId == incharge.Id);

        if (!isTeamMember) return Forbid();

        application.InchargeId = incharge.Id;
        application.InchargeApprovalStatus = "Rejected";
        application.InchargeApprovedAt = DateTime.UtcNow;
        application.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["ErrorMessage"] = $"Leave request {application.LeaveNumber} rejected.";
        return RedirectToAction(nameof(TeamLeaveRequests));
    }

    // ── Leave Flexis Chart ───────────────────────────────────────────────────

    public async Task<IActionResult> LeaveFlexisChart()
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new LeaveFlexisChartViewModel());
        }

        var vm = await BuildLeaveFlexisChartViewModelAsync(incharge);
        ViewData["Title"] = "Leave / Flexis Chart";
        return View(vm);
    }

    // ── In Out Timings ──────────────────────────────────────────────────────

    public async Task<IActionResult> InOutTimings()
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(await BuildEmptyInOutViewModelAsync(null));
        }
        var vm = await BuildEmptyInOutViewModelAsync(incharge);
        ViewData["Title"] = "In Out Timings";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InOutTimings(InchargeInOutTimingsViewModel form)
    {
        ViewData["Title"] = "In Out Timings";
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(await BuildEmptyInOutViewModelAsync(null));
        }

        var vm = await BuildEmptyInOutViewModelAsync(incharge);
        vm.SelectedMonth = form.SelectedMonth;
        vm.SelectedYear = form.SelectedYear;
        vm.IsPayrollWise = form.IsPayrollWise;
        vm.IsMonthWise = form.IsMonthWise;

        if (form.SelectedEmployeeId.HasValue)
        {
            var teamMember = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Designation)
                .FirstOrDefaultAsync(e => e.Id == form.SelectedEmployeeId.Value && e.InchargeEmployeeId == incharge.Id);

            if (teamMember != null)
            {
                vm.SelectedEmployeeId = teamMember.Id;
                vm.SelectedEmployeeName = teamMember.Name;
                vm.SelectedEmployeeNo = teamMember.EmployeeNo;
                vm.SelectedEmployeeDept = teamMember.Department?.Name;
                vm.SelectedEmployeeDesignation = teamMember.Designation?.Name;

                var startDate = new DateTime(form.SelectedYear, form.SelectedMonth, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

                var attendances = await _context.Attendances
                    .Where(a => a.EmployeeId == teamMember.Id &&
                                a.PunchDateTime >= startDate &&
                                a.PunchDateTime <= endDate)
                    .OrderBy(a => a.PunchDateTime)
                    .ToListAsync();

                vm.InOutTimingsItems = ProcessInOutAttendances(attendances);
            }
            else
            {
                TempData["ErrorMessage"] = "Selected employee is not in your team.";
            }
        }

        return View(vm);
    }

    private async Task<InchargeInOutTimingsViewModel> BuildEmptyInOutViewModelAsync(Employee? incharge)
    {
        var currentYear = DateTime.Now.Year;
        var months = new List<InChargeMonthItem>
        {
            new() { Value = 1, Name = "January" },  new() { Value = 2, Name = "February" },
            new() { Value = 3, Name = "March" },     new() { Value = 4, Name = "April" },
            new() { Value = 5, Name = "May" },       new() { Value = 6, Name = "June" },
            new() { Value = 7, Name = "July" },      new() { Value = 8, Name = "August" },
            new() { Value = 9, Name = "September" }, new() { Value = 10, Name = "October" },
            new() { Value = 11, Name = "November" }, new() { Value = 12, Name = "December" }
        };

        var teamItems = new List<SelectListItem>();
        if (incharge != null)
        {
            var members = await _context.Employees
                .Where(e => e.InchargeEmployeeId == incharge.Id)
                .OrderBy(e => e.Name)
                .Select(e => new { e.Id, e.Name, e.EmployeeNo })
                .ToListAsync();

            teamItems = members
                .Select(e => new SelectListItem($"{e.Name} ({e.EmployeeNo})", e.Id.ToString()))
                .ToList();
        }

        return new InchargeInOutTimingsViewModel
        {
            SelectedMonth = DateTime.Now.Month,
            SelectedYear = currentYear,
            IsPayrollWise = true,
            AvailableYears = Enumerable.Range(currentYear - 5, 7).ToList(),
            AvailableMonths = months,
            EmployeeOptions = new SelectList(teamItems, "Value", "Text")
        };
    }

    private static List<InOutTimingsItem> ProcessInOutAttendances(List<Attendance> attendances)
    {
        var items = new List<InOutTimingsItem>();
        var srNo = 1;

        var groupedByDate = attendances
            .GroupBy(a => a.PunchDateTime.Date)
            .OrderBy(g => g.Key);

        foreach (var group in groupedByDate)
        {
            var dayRecords = group.OrderBy(a => a.PunchDateTime).ToList();
            var timeIn = dayRecords.FirstOrDefault(a => a.PunchType == "In")?.PunchDateTime.TimeOfDay;
            var timeOut = dayRecords.LastOrDefault(a => a.PunchType == "Out")?.PunchDateTime.TimeOfDay;

            TimeSpan? extraHours = null;
            if (timeIn.HasValue && timeOut.HasValue)
            {
                var total = timeOut.Value - timeIn.Value;
                var standard = TimeSpan.FromHours(8);
                if (total > standard) extraHours = total - standard;
            }

            var status = (timeIn.HasValue, timeOut.HasValue) switch
            {
                (false, false) => "Absent",
                (true, false)  => "Half Day (No Out)",
                (false, true)  => "Half Day (No In)",
                _ => (timeOut!.Value - timeIn!.Value) < TimeSpan.FromHours(4) ? "Half Day" : "Present"
            };

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

        return items;
    }

    // ── Leave Sheet ──────────────────────────────────────────────────────────

    public async Task<IActionResult> LeaveSheet()
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(await BuildEmptyLeaveSheetAsync(null));
        }
        ViewData["Title"] = "Leave Sheet";
        return View(await BuildEmptyLeaveSheetAsync(incharge));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveSheet(LeaveSheetViewModel form)
    {
        ViewData["Title"] = "Leave Sheet";
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(await BuildEmptyLeaveSheetAsync(null));
        }

        var vm = await BuildEmptyLeaveSheetAsync(incharge);
        vm.SelectedEmployeeId = form.SelectedEmployeeId;
        vm.SelectedYear       = form.SelectedYear;

        if (!form.SelectedEmployeeId.HasValue)
        {
            ModelState.AddModelError("SelectedEmployeeId", "Please select an employee.");
            return View(vm);
        }

        var member = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Category)
            .Include(e => e.Leave)
                .ThenInclude(l => l!.LeaveDetails)
            .FirstOrDefaultAsync(e => e.Id == form.SelectedEmployeeId.Value
                                   && e.InchargeEmployeeId == incharge.Id);

        if (member == null)
        {
            TempData["ErrorMessage"] = "Selected employee is not in your team.";
            return View(vm);
        }

        vm.EmployeeName        = member.Name;
        vm.EmployeeNo          = member.EmployeeNo;
        vm.Department          = member.Department?.Name;
        vm.Designation         = member.Designation?.Name;
        vm.EmployeeType        = member.EmployeeType;
        vm.Category            = member.Category?.Name;
        vm.JoiningDate         = member.JoiningDate;

        var periodFrom = new DateTime(form.SelectedYear, 1, 1);
        var periodTo   = new DateTime(form.SelectedYear, 12, 31, 23, 59, 59);

        var applications = await _context.LeaveApplications
            .Where(la => la.EmployeeId == member.Id
                      && la.Status == "Approved"
                      && la.StartDate <= periodTo
                      && la.EndDate >= periodFrom)
            .OrderBy(la => la.StartDate)
            .ToListAsync();

        var leaveDetails = member.Leave?.LeaveDetails ?? new List<LeaveDetail>();

        foreach (var detail in leaveDetails.OrderBy(d => d.LeaveType))
        {
            var typeApps = applications
                .Where(a => a.LeaveType == detail.LeaveType)
                .ToList();

            var row = new LeaveSheetRow
            {
                LeaveType    = detail.LeaveType,
                Allowed      = detail.NoOfDays,
                Taken        = typeApps.Sum(a => a.TotalDays),
                CarryForward = detail.CarryForward,
                Applications = typeApps.Select(a => new LeaveSheetApplication
                {
                    LeaveNumber = a.LeaveNumber,
                    StartDate   = a.StartDate,
                    EndDate     = a.EndDate,
                    TotalDays   = a.TotalDays,
                    Reason      = a.Reason,
                    Status      = a.Status
                }).ToList()
            };

            vm.Rows.Add(row);
        }

        // Also surface any leave types taken that are NOT in the policy (miscellaneous)
        var knownTypes = leaveDetails.Select(d => d.LeaveType).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknownTypeGroups = applications
            .Where(a => !knownTypes.Contains(a.LeaveType))
            .GroupBy(a => a.LeaveType);

        foreach (var grp in unknownTypeGroups)
        {
            vm.Rows.Add(new LeaveSheetRow
            {
                LeaveType    = grp.Key,
                Allowed      = 0,
                Taken        = grp.Sum(a => a.TotalDays),
                CarryForward = false,
                Applications = grp.Select(a => new LeaveSheetApplication
                {
                    LeaveNumber = a.LeaveNumber,
                    StartDate   = a.StartDate,
                    EndDate     = a.EndDate,
                    TotalDays   = a.TotalDays,
                    Reason      = a.Reason,
                    Status      = a.Status
                }).ToList()
            });
        }

        return View(vm);
    }

    private async Task<LeaveSheetViewModel> BuildEmptyLeaveSheetAsync(Employee? incharge)
    {
        var teamItems = new List<SelectListItem>();
        if (incharge != null)
        {
            var members = await _context.Employees
                .Where(e => e.InchargeEmployeeId == incharge.Id && e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new { e.Id, e.Name, e.EmployeeNo })
                .ToListAsync();

            teamItems = members
                .Select(e => new SelectListItem($"{e.Name}  ({e.EmployeeNo})", e.Id.ToString()))
                .ToList();
        }

        var currentYear = DateTime.Now.Year;
        return new LeaveSheetViewModel
        {
            SelectedYear    = currentYear,
            AvailableYears  = Enumerable.Range(currentYear - 5, 7).ToList(),
            EmployeeOptions = new SelectList(teamItems, "Value", "Text")
        };
    }

    // ── Update Calendar ──────────────────────────────────────────────────────

    public async Task<IActionResult> UpdateCalendar(int? employeeId, int? month, int? year)
    {
        ViewData["Title"] = "Update Calendar";
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(await BuildEmptyUpdateCalendarAsync(null));
        }

        var vm = await BuildEmptyUpdateCalendarAsync(incharge);
        vm.SelectedMonth = month ?? DateTime.Now.Month;
        vm.SelectedYear  = year  ?? DateTime.Now.Year;

        if (!employeeId.HasValue)
            return View(vm);

        var member = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .FirstOrDefaultAsync(e => e.Id == employeeId.Value
                                   && e.InchargeEmployeeId == incharge.Id);

        if (member == null)
        {
            TempData["ErrorMessage"] = "Selected employee is not in your team.";
            return View(vm);
        }

        vm.SelectedEmployeeId = member.Id;
        vm.EmployeeName   = member.Name;
        vm.EmployeeNo     = member.EmployeeNo;
        vm.Department     = member.Department?.Name;
        vm.Designation    = member.Designation?.Name;

        var startDate = new DateTime(vm.SelectedYear, vm.SelectedMonth, 1);
        var endDate   = startDate.AddMonths(1).AddDays(-1);

        // Load saved statuses from DB for this employee+month
        var savedDays = await _context.EmployeeCalendarDays
            .Where(d => d.EmployeeId == member.Id
                     && d.Date >= startDate && d.Date <= endDate)
            .ToDictionaryAsync(d => d.Date.Date, d => d.DayStatus);

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            string defaultStatus = date.DayOfWeek == DayOfWeek.Sunday ? "Sunday" : "Working";
            vm.CalendarDays.Add(new CalendarDayEntry
            {
                Date      = date,
                DayStatus = savedDays.TryGetValue(date.Date, out var saved) ? saved : defaultStatus
            });
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCalendar(IFormCollection form)
    {
        var incharge = await GetLoggedInEmployeeAsync(User.Identity?.Name);
        if (incharge == null)
        {
            TempData["ErrorMessage"] = "Employee record not found.";
            return RedirectToAction(nameof(UpdateCalendar));
        }

        if (!int.TryParse(form["employeeId"], out int employeeId) ||
            !int.TryParse(form["month"],      out int month)      ||
            !int.TryParse(form["year"],       out int year))
        {
            TempData["ErrorMessage"] = "Invalid form data.";
            return RedirectToAction(nameof(UpdateCalendar));
        }

        var member = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == employeeId
                                   && e.InchargeEmployeeId == incharge.Id);

        if (member == null)
        {
            TempData["ErrorMessage"] = "Selected employee is not in your team.";
            return RedirectToAction(nameof(UpdateCalendar));
        }

        var startDate = new DateTime(year, month, 1);
        var endDate   = startDate.AddMonths(1).AddDays(-1);

        // Load existing records for upsert
        var existingRows = await _context.EmployeeCalendarDays
            .Where(d => d.EmployeeId == employeeId
                     && d.Date >= startDate && d.Date <= endDate)
            .ToDictionaryAsync(d => d.Date.Date);

        var now = DateTime.UtcNow;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var key    = $"status_{date:yyyy-MM-dd}";
            var status = form[key].ToString();
            if (string.IsNullOrWhiteSpace(status)) continue;

            if (existingRows.TryGetValue(date.Date, out var row))
            {
                row.DayStatus            = status;
                row.UpdatedByEmployeeId  = incharge.Id;
                row.UpdatedAtUtc         = now;
            }
            else
            {
                _context.EmployeeCalendarDays.Add(new EmployeeCalendarDay
                {
                    CompanyId           = member.CompanyId,
                    EmployeeId          = employeeId,
                    Date                = date.Date,
                    DayStatus           = status,
                    UpdatedByEmployeeId = incharge.Id,
                    UpdatedAtUtc        = now
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Calendar updated for {member.Name} — {new DateTime(year, month, 1):MMMM yyyy}.";
        return RedirectToAction(nameof(UpdateCalendar),
            new { employeeId, month, year });
    }

    private async Task<UpdateCalendarViewModel> BuildEmptyUpdateCalendarAsync(Employee? incharge)
    {
        var teamItems = new List<SelectListItem>();
        if (incharge != null)
        {
            var members = await _context.Employees
                .Where(e => e.InchargeEmployeeId == incharge.Id && e.IsActive)
                .OrderBy(e => e.Name)
                .Select(e => new { e.Id, e.Name, e.EmployeeNo })
                .ToListAsync();

            teamItems = members
                .Select(e => new SelectListItem($"{e.Name}  ({e.EmployeeNo})", e.Id.ToString()))
                .ToList();
        }

        var currentYear = DateTime.Now.Year;
        return new UpdateCalendarViewModel
        {
            SelectedMonth   = DateTime.Now.Month,
            SelectedYear    = currentYear,
            AvailableYears  = Enumerable.Range(currentYear - 3, 5).ToList(),
            AvailableMonths = BuildCalendarMonthList(),
            EmployeeOptions = new SelectList(teamItems, "Value", "Text")
        };
    }

    private static List<CalendarMonthItem> BuildCalendarMonthList() => new()
    {
        new() { Value = 1,  Name = "January"   }, new() { Value = 2,  Name = "February"  },
        new() { Value = 3,  Name = "March"      }, new() { Value = 4,  Name = "April"     },
        new() { Value = 5,  Name = "May"        }, new() { Value = 6,  Name = "June"      },
        new() { Value = 7,  Name = "July"       }, new() { Value = 8,  Name = "August"    },
        new() { Value = 9,  Name = "September"  }, new() { Value = 10, Name = "October"   },
        new() { Value = 11, Name = "November"   }, new() { Value = 12, Name = "December"  }
    };

    // ── Private Helpers ──────────────────────────────────────────────────────

    private async Task<Employee?> GetLoggedInEmployeeAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return null;

        return await _context.Employees
            .Include(e => e.Leave)
                .ThenInclude(l => l!.LeaveDetails)
            .Include(e => e.Leave)
                .ThenInclude(l => l!.LeavePolicy)
            .FirstOrDefaultAsync(e => e.EmployeeNo == userName || e.Email == userName);
    }

    private async Task<CalendarViewModel> BuildCalendarViewModelAsync(Employee incharge, int month, int year, int? filterEmployeeId = null, string? viewMode = "team")
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var events = new List<CalendarEvent>();

        if (filterEmployeeId.HasValue)
        {
            // Show only the selected team member's leaves
            var empLeaves = await _context.LeaveApplications
                .Where(la => la.EmployeeId == filterEmployeeId.Value &&
                             la.Status == "Approved" &&
                             la.StartDate <= endDate && la.EndDate >= startDate)
                .ToListAsync();

            foreach (var la in empLeaves)
            {
                var s = la.StartDate > startDate ? la.StartDate : startDate;
                var e = la.EndDate < endDate ? la.EndDate : endDate;
                for (var d = s; d <= e; d = d.AddDays(1))
                    events.Add(new CalendarEvent { Date = d, Title = $"{la.LeaveType} Leave", Type = "TeamLeave", Color = "#0d6efd", Description = la.Reason });
            }
        }
        else if (viewMode?.ToLowerInvariant() == "myself")
        {
            // Show only incharge's own leaves
            var ownLeaves = await _context.LeaveApplications
                .Where(la => la.EmployeeId == incharge.Id &&
                             la.Status == "Approved" &&
                             la.StartDate <= endDate && la.EndDate >= startDate)
                .ToListAsync();

            foreach (var la in ownLeaves)
            {
                var s = la.StartDate > startDate ? la.StartDate : startDate;
                var e = la.EndDate < endDate ? la.EndDate : endDate;
                for (var d = s; d <= e; d = d.AddDays(1))
                    events.Add(new CalendarEvent { Date = d, Title = $"{la.LeaveType} Leave (Me)", Type = "Leave", Color = "#fd7e14", Description = la.Reason });
            }

            // Own birthday
            if (incharge.DateOfBirth.HasValue && incharge.DateOfBirth.Value.Month == month)
            {
                var bday = new DateTime(year, month, incharge.DateOfBirth.Value.Day);
                if (bday >= startDate && bday <= endDate)
                    events.Add(new CalendarEvent { Date = bday, Title = "Your Birthday", Type = "Birthday", Color = "#e83e8c", Description = "Happy Birthday!" });
            }
        }
        else
        {
            // Default / Team: show incharge + all team members' leaves
            var ownLeaves = await _context.LeaveApplications
                .Where(la => la.EmployeeId == incharge.Id &&
                             la.Status == "Approved" &&
                             la.StartDate <= endDate && la.EndDate >= startDate)
                .ToListAsync();

            foreach (var la in ownLeaves)
            {
                var s = la.StartDate > startDate ? la.StartDate : startDate;
                var e = la.EndDate < endDate ? la.EndDate : endDate;
                for (var d = s; d <= e; d = d.AddDays(1))
                    events.Add(new CalendarEvent { Date = d, Title = $"{la.LeaveType} Leave (Me)", Type = "Leave", Color = "#fd7e14", Description = la.Reason });
            }

            // Show all team members' leaves
            var teamIds = await _context.Employees
                .Where(e => e.InchargeEmployeeId == incharge.Id)
                .Select(e => new { e.Id, e.Name })
                .ToListAsync();

            if (teamIds.Any())
            {
                var ids = teamIds.Select(t => t.Id).ToList();
                var teamLeaves = await _context.LeaveApplications
                    .Where(la => ids.Contains(la.EmployeeId) &&
                                 la.Status == "Approved" &&
                                 la.StartDate <= endDate && la.EndDate >= startDate)
                    .ToListAsync();

                foreach (var la in teamLeaves)
                {
                    var memberName = teamIds.FirstOrDefault(t => t.Id == la.EmployeeId)?.Name ?? "Team Member";
                    var s = la.StartDate > startDate ? la.StartDate : startDate;
                    var e = la.EndDate < endDate ? la.EndDate : endDate;
                    for (var d = s; d <= e; d = d.AddDays(1))
                        events.Add(new CalendarEvent { Date = d, Title = $"{la.LeaveType} ({memberName})", Type = "TeamLeave", Color = "#0d6efd", Description = la.Reason });
                }
            }

            // Own birthday
            if (incharge.DateOfBirth.HasValue && incharge.DateOfBirth.Value.Month == month)
            {
                var bday = new DateTime(year, month, incharge.DateOfBirth.Value.Day);
                if (bday >= startDate && bday <= endDate)
                    events.Add(new CalendarEvent { Date = bday, Title = "Your Birthday", Type = "Birthday", Color = "#e83e8c", Description = "Happy Birthday!" });
            }
        }

        // Company holidays always shown
        var markDays = await _context.MarkDays
            .Where(md => md.CompanyId == incharge.CompanyId &&
                         md.FromDate <= endDate && md.ToDate >= startDate)
            .ToListAsync();

        foreach (var md in markDays)
        {
            var s = md.FromDate > startDate ? md.FromDate : startDate;
            var e = md.ToDate < endDate ? md.ToDate : endDate;
            var title = md.IsGazettedHoliday ? "Gazetted Holiday"
                      : md.IsProvincialHoliday ? "Provincial Holiday"
                      : md.IsEid ? "Eid Holiday"
                      : md.IsStrike ? "Strike Day"
                      : md.IsOn ? "Working Day"
                      : md.IsOff ? "Off Day"
                      : "Holiday";
            for (var d = s; d <= e; d = d.AddDays(1))
                events.Add(new CalendarEvent { Date = d, Title = title, Type = "Holiday", Color = "#198754", Description = title });
        }

        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 2, 5).ToList();
        var availableMonths = BuildMonthList();
        var monthOptions = availableMonths.Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Name, Selected = m.Value == month }).ToList();
        var yearOptions = availableYears.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = y == year }).ToList();

        return new CalendarViewModel
        {
            EmployeeId = incharge.Id,
            EmployeeName = incharge.Name,
            EmployeeNo = incharge.EmployeeNo,
            CurrentMonth = month,
            CurrentYear = year,
            Events = events.OrderBy(ev => ev.Date).ToList(),
            AvailableYears = availableYears,
            AvailableMonths = availableMonths,
            MonthOptions = new SelectList(monthOptions, "Value", "Text"),
            YearOptions = new SelectList(yearOptions, "Value", "Text")
        };
    }

    private CalendarViewModel CreateEmptyCalendarViewModel()
    {
        var currentYear = DateTime.Now.Year;
        var availableYears = Enumerable.Range(currentYear - 2, 5).ToList();
        var availableMonths = BuildMonthList();
        var monthOptions = availableMonths.Select(m => new SelectListItem { Value = m.Value.ToString(), Text = m.Name, Selected = m.Value == DateTime.Now.Month }).ToList();
        var yearOptions = availableYears.Select(y => new SelectListItem { Value = y.ToString(), Text = y.ToString(), Selected = y == currentYear }).ToList();

        return new CalendarViewModel
        {
            CurrentMonth = DateTime.Now.Month,
            CurrentYear = currentYear,
            AvailableYears = availableYears,
            AvailableMonths = availableMonths,
            MonthOptions = new SelectList(monthOptions, "Value", "Text"),
            YearOptions = new SelectList(yearOptions, "Value", "Text")
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

    private static List<MonthItem> BuildMonthList() => new()
    {
        new() { Value = 1, Name = "January" },   new() { Value = 2, Name = "February" },
        new() { Value = 3, Name = "March" },      new() { Value = 4, Name = "April" },
        new() { Value = 5, Name = "May" },        new() { Value = 6, Name = "June" },
        new() { Value = 7, Name = "July" },       new() { Value = 8, Name = "August" },
        new() { Value = 9, Name = "September" },  new() { Value = 10, Name = "October" },
        new() { Value = 11, Name = "November" },  new() { Value = 12, Name = "December" }
    };
}
