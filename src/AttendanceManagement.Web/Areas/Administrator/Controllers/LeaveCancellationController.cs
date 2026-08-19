using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.LeaveCancellations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class LeaveCancellationController : Controller
{
    private readonly AttendanceDbContext _context;

    public LeaveCancellationController(AttendanceDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var viewModel = new LeaveCancellationViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Search(string leaveNumber)
    {
        if (string.IsNullOrWhiteSpace(leaveNumber))
        {
            ModelState.AddModelError("LeaveNumber", "Leave number is required");
            return View("Index", new LeaveCancellationViewModel());
        }

        var leaveApplication = await _context.LeaveApplications
            .Include(la => la.Employee)
            .Include(la => la.Incharge)
            .Include(la => la.CancelledByEmployee)
            .FirstOrDefaultAsync(la => la.LeaveNumber == leaveNumber);

        if (leaveApplication is null)
        {
            ModelState.AddModelError("LeaveNumber", "Leave not found with this number");
            return View("Index", new LeaveCancellationViewModel { LeaveNumber = leaveNumber });
        }

        var viewModel = new LeaveCancellationViewModel
        {
            Id = leaveApplication.Id,
            LeaveNumber = leaveApplication.LeaveNumber,
            EmployeeName = leaveApplication.Employee?.Name,
            EmployeeNo = leaveApplication.Employee?.EmployeeNo,
            LeaveType = leaveApplication.LeaveType,
            StartDate = leaveApplication.StartDate,
            EndDate = leaveApplication.EndDate,
            TotalDays = leaveApplication.TotalDays,
            Reason = leaveApplication.Reason,
            Status = leaveApplication.Status,
            InchargeApprovalStatus = leaveApplication.InchargeApprovalStatus,
            InchargeName = leaveApplication.Incharge?.Name
        };

        // Check if leave can be cancelled
        // Rules:
        // 1. Must be approved by Incharge
        // 2. Leave type cannot be Official, Short, or Withoutpay
        // 3. Status must not already be Cancelled
        // 4. InchargeApprovalStatus must be Approved

        var excludedLeaveTypes = new[] { "Official", "Short", "Withoutpay", "Without Pay" };
        var isExcludedType = excludedLeaveTypes.Contains(leaveApplication.LeaveType, StringComparer.OrdinalIgnoreCase);

        if (leaveApplication.Status == "Cancelled")
        {
            viewModel.CanCancel = false;
            viewModel.CannotCancelReason = "This leave has already been cancelled.";
        }
        else if (isExcludedType)
        {
            viewModel.CanCancel = false;
            viewModel.CannotCancelReason = $"Leave type '{leaveApplication.LeaveType}' cannot be cancelled.";
        }
        else if (leaveApplication.InchargeApprovalStatus != "Approved")
        {
            viewModel.CanCancel = false;
            viewModel.CannotCancelReason = "Only leaves approved by Incharge can be cancelled.";
        }
        else
        {
            viewModel.CanCancel = true;
        }

        return View("Index", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string cancellationReason)
    {
        var leaveApplication = await _context.LeaveApplications.FindAsync(id);
        if (leaveApplication is null)
        {
            return NotFound();
        }

        // Validate cancellation again
        var excludedLeaveTypes = new[] { "Official", "Short", "Withoutpay", "Without Pay" };
        var isExcludedType = excludedLeaveTypes.Contains(leaveApplication.LeaveType, StringComparer.OrdinalIgnoreCase);

        if (leaveApplication.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = "This leave has already been cancelled.";
            return RedirectToAction(nameof(Index));
        }

        if (isExcludedType)
        {
            TempData["ErrorMessage"] = $"Leave type '{leaveApplication.LeaveType}' cannot be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        if (leaveApplication.InchargeApprovalStatus != "Approved")
        {
            TempData["ErrorMessage"] = "Only leaves approved by Incharge can be cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // Cancel the leave
        leaveApplication.Status = "Cancelled";
        leaveApplication.CancelledAt = DateTime.UtcNow;
        leaveApplication.CancellationReason = cancellationReason;
        leaveApplication.UpdatedAtUtc = DateTime.UtcNow;

        _context.LeaveApplications.Update(leaveApplication);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Leave cancelled successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        return RedirectToAction(nameof(Index));
    }
}
