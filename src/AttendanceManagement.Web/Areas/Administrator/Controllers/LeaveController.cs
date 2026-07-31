using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Leaves;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class LeaveController : Controller
{
    private readonly AttendanceDbContext _context;

    public LeaveController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Leaves
            .Include(l => l.Company)
            .Include(l => l.Category)
            .Include(l => l.LeavePolicy)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l => l.Description != null && l.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var leaves = await query.ToListAsync();
        var viewModels = leaves.Select(l => new LeaveViewModel
        {
            Id = l.Id,
            Name = l.Name,
            CompanyId = l.CompanyId,
            CompanyName = l.Company?.Name,
            CategoryId = l.CategoryId,
            CategoryName = l.Category?.Name,
            LeavePolicyId = l.LeavePolicyId,
            LeavePolicyName = l.LeavePolicy?.Name,
            Description = l.Description,
            IsActive = l.IsActive,
            LeaveDetails = l.LeaveDetails.Select(ld => new LeaveDetailViewModel
            {
                Id = ld.Id,
                LeaveType = ld.LeaveType,
                NoOfDays = ld.NoOfDays,
                CarryForward = ld.CarryForward
            }).ToList()
        }).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Create()
    {
        // Get current user's company - for now, we'll use the first company as default
        var companies = await _context.Companies.OrderBy(c => c.Name).ToListAsync();
        var company = companies.FirstOrDefault();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";

        var viewModel = new LeaveViewModel
        {
            CompanyId = companyId,
            CompanyName = companyName,
            IsActive = true,
            LeaveDetails = GetDefaultLeaveDetails()
        };
        ViewBag.CompanyId = companyId;
        ViewBag.CompanyName = companyName;
        await PopulateLookupsAsync(viewModel);
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(viewModel);
            return View("Edit", viewModel);
        }

        var leave = new Leave
        {
            Name = viewModel.Name,
            CompanyId = viewModel.CompanyId,
            CategoryId = viewModel.CategoryId,
            LeavePolicyId = viewModel.LeavePolicyId,
            Description = viewModel.Description,
            IsActive = viewModel.IsActive,
            LeaveDetails = viewModel.LeaveDetails.Select(ld => new LeaveDetail
            {
                LeaveType = ld.LeaveType,
                NoOfDays = ld.NoOfDays,
                CarryForward = ld.CarryForward
            }).ToList()
        };

        _context.Leaves.Add(leave);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var leave = await _context.Leaves
            .Include(l => l.LeaveDetails)
            .Include(l => l.Company)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (leave is null)
        {
            return NotFound();
        }

        var viewModel = new LeaveViewModel
        {
            Id = leave.Id,
            Name = leave.Name,
            CompanyId = leave.CompanyId,
            CompanyName = leave.Company?.Name,
            CategoryId = leave.CategoryId,
            LeavePolicyId = leave.LeavePolicyId,
            Description = leave.Description,
            IsActive = leave.IsActive,
            LeaveDetails = leave.LeaveDetails.Select(ld => new LeaveDetailViewModel
            {
                Id = ld.Id,
                LeaveType = ld.LeaveType,
                NoOfDays = ld.NoOfDays,
                CarryForward = ld.CarryForward
            }).ToList()
        };

        ViewBag.CompanyId = leave.CompanyId;
        ViewBag.CompanyName = leave.Company?.Name;
        await PopulateLookupsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeaveViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(viewModel);
            return View(viewModel);
        }

        var leave = await _context.Leaves
            .Include(l => l.LeaveDetails)
            .FirstOrDefaultAsync(l => l.Id == id);
        
        if (leave is null)
        {
            return NotFound();
        }

        leave.Name = viewModel.Name;
        leave.CategoryId = viewModel.CategoryId;
        leave.LeavePolicyId = viewModel.LeavePolicyId;
        leave.Description = viewModel.Description;
        leave.IsActive = viewModel.IsActive;

        // Update leave details
        var existingDetailIds = leave.LeaveDetails.Select(ld => ld.Id).ToHashSet();
        var viewModelDetailIds = viewModel.LeaveDetails.Where(ld => ld.Id > 0).Select(ld => ld.Id).ToHashSet();

        // Remove deleted details
        var detailsToRemove = leave.LeaveDetails.Where(ld => !viewModelDetailIds.Contains(ld.Id)).ToList();
        foreach (var detail in detailsToRemove)
        {
            _context.LeaveDetails.Remove(detail);
        }

        // Update or add details
        foreach (var detailViewModel in viewModel.LeaveDetails)
        {
            if (detailViewModel.Id > 0 && existingDetailIds.Contains(detailViewModel.Id))
            {
                var existingDetail = leave.LeaveDetails.First(ld => ld.Id == detailViewModel.Id);
                existingDetail.LeaveType = detailViewModel.LeaveType;
                existingDetail.NoOfDays = detailViewModel.NoOfDays;
                existingDetail.CarryForward = detailViewModel.CarryForward;
            }
            else
            {
                leave.LeaveDetails.Add(new LeaveDetail
                {
                    LeaveType = detailViewModel.LeaveType,
                    NoOfDays = detailViewModel.NoOfDays,
                    CarryForward = detailViewModel.CarryForward
                });
            }
        }

        _context.Leaves.Update(leave);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _context.Leaves.FindAsync(id);
        if (leave is null)
        {
            return NotFound();
        }

        _context.Leaves.Remove(leave);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static List<LeaveDetailViewModel> GetDefaultLeaveDetails()
    {
        return new List<LeaveDetailViewModel>
        {
            new LeaveDetailViewModel { LeaveType = "Annual Leave", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Sick Leave", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Casual Leave", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Maternity Leave", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Default", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Default", NoOfDays = 0, CarryForward = false },
            new LeaveDetailViewModel { LeaveType = "Default", NoOfDays = 0, CarryForward = false }
        };
    }

    private async Task PopulateLookupsAsync(LeaveViewModel viewModel)
    {
        var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        var leavePolicies = await _context.LeavePolicies.Where(lp => lp.Status == "Active").OrderBy(lp => lp.Name).ToListAsync();

        viewModel.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == viewModel.CategoryId)).ToList();
        viewModel.LeavePolicies = leavePolicies.Select(lp => new SelectListItem(lp.Name, lp.Id.ToString(), lp.Id == viewModel.LeavePolicyId)).ToList();
    }
}
