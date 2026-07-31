using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.LeavePolicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class LeavePolicyController : Controller
{
    private readonly AttendanceDbContext _context;

    public LeavePolicyController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.LeavePolicies.Include(l => l.Company).AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(l => l.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var leavePolicies = await query.ToListAsync();
        var viewModels = leavePolicies.Select(l => new LeavePolicyViewModel
        {
            Id = l.Id,
            Name = l.Name,
            Status = l.Status,
            CompanyId = l.CompanyId,
            CompanyName = l.Company?.Name
        }).ToList();

        return View(viewModels);
    }

    public async Task<IActionResult> Create()
    {
        // Get current user's company - for now, we'll use the first company as default
        // In a real scenario, this would come from the authenticated user's company
        var companies = await _context.Companies.OrderBy(c => c.Name).ToListAsync();
        var company = companies.FirstOrDefault();
        var companyId = company?.Id ?? Guid.Empty;
        var companyName = company?.Name ?? "Default Company";

        var viewModel = new LeavePolicyViewModel
        {
            CompanyId = companyId,
            CompanyName = companyName
        };
        ViewBag.CompanyId = companyId;
        ViewBag.CompanyName = companyName;
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeavePolicyViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCompaniesAsync(viewModel);
            return View("Edit", viewModel);
        }

        var leavePolicy = new LeavePolicy
        {
            Name = viewModel.Name,
            Status = viewModel.Status,
            CompanyId = viewModel.CompanyId
        };

        _context.LeavePolicies.Add(leavePolicy);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave Policy created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var leavePolicy = await _context.LeavePolicies.Include(l => l.Company).FirstOrDefaultAsync(l => l.Id == id);
        if (leavePolicy is null)
        {
            return NotFound();
        }

        var viewModel = new LeavePolicyViewModel
        {
            Id = leavePolicy.Id,
            Name = leavePolicy.Name,
            Status = leavePolicy.Status,
            CompanyId = leavePolicy.CompanyId,
            CompanyName = leavePolicy.Company?.Name
        };

        await PopulateCompaniesAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeavePolicyViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateCompaniesAsync(viewModel);
            return View(viewModel);
        }

        var leavePolicy = await _context.LeavePolicies.FindAsync(id);
        if (leavePolicy is null)
        {
            return NotFound();
        }

        leavePolicy.Name = viewModel.Name;
        leavePolicy.Status = viewModel.Status;
        leavePolicy.CompanyId = viewModel.CompanyId;

        _context.LeavePolicies.Update(leavePolicy);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave Policy updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var leavePolicy = await _context.LeavePolicies.FindAsync(id);
        if (leavePolicy is null)
        {
            return NotFound();
        }

        _context.LeavePolicies.Remove(leavePolicy);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Leave Policy deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCompaniesAsync(LeavePolicyViewModel viewModel)
    {
        var companies = await _context.Companies.OrderBy(c => c.Name).ToListAsync();
        viewModel.Companies = companies.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == viewModel.CompanyId)).ToList();
    }
}
