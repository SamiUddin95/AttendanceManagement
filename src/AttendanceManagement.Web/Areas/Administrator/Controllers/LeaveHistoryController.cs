using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.LeaveHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class LeaveHistoryController : Controller
{
    private readonly AttendanceDbContext _context;

    public LeaveHistoryController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Get logged-in employee by matching User.Identity.Name with EmployeeNo or Email
        var userName = User.Identity?.Name;
        var employee = await GetLoggedInEmployeeAsync(userName);

        if (employee == null)
        {
            TempData["ErrorMessage"] = "Employee record not found for current user.";
            return View(new LeaveHistoryViewModel());
        }

        var viewModel = await BuildLeaveHistoryViewModelAsync(employee);
        return View(viewModel);
    }

    private async Task<Employee?> GetLoggedInEmployeeAsync(string? userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return null;

        // Try to match by EmployeeNo first, then by Email
        var employee = await _context.Employees
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.EmployeeNo == userName || e.Email == userName);

        return employee;
    }

    private async Task<LeaveHistoryViewModel> BuildLeaveHistoryViewModelAsync(Employee employee)
    {
        var leaveApplications = await _context.LeaveApplications
            .Where(la => la.EmployeeId == employee.Id)
            .OrderByDescending(la => la.CreatedAtUtc)
            .ToListAsync();

        var items = leaveApplications.Select(la => new LeaveHistoryItem
        {
            LeaveId = la.Id,
            LeaveNumber = la.LeaveNumber,
            LeaveType = la.LeaveType,
            DateApplied = la.CreatedAtUtc,
            FromDate = la.StartDate,
            ToDate = la.EndDate,
            TotalDays = la.TotalDays,
            Status = la.Status,
            ModifiedDate = la.UpdatedAtUtc,
            Reason = la.Reason,
            InchargeApprovedAt = la.InchargeApprovedAt,
            InchargeApprovalStatus = la.InchargeApprovalStatus,
            CancelledAt = la.CancelledAt,
            CancellationReason = la.CancellationReason
        }).ToList();

        return new LeaveHistoryViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.Name,
            EmployeeNo = employee.EmployeeNo,
            LeaveHistoryItems = items
        };
    }
}
