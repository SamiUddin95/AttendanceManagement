using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.SuperUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.SuperUser.Controllers;

[Area("SuperUser")]
[Authorize(Roles = "SuperUser,Administrator")]
public class ModuleController : Controller
{
    private readonly AttendanceDbContext _context;

    public ModuleController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var modules = await _context.Modules
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return View(modules);
    }

    public IActionResult Create()
    {
        return View(new Module());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Module module)
    {
        if (!ModelState.IsValid)
        {
            return View(module);
        }

        module.CreatedAtUtc = DateTime.UtcNow;
        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Module created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var module = await _context.Modules.FindAsync(id);
        if (module == null)
        {
            return NotFound();
        }

        return View(module);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Module module)
    {
        if (id != module.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(module);
        }

        var existing = await _context.Modules.FindAsync(id);
        if (existing == null)
        {
            return NotFound();
        }

        existing.Name = module.Name;
        existing.DisplayName = module.DisplayName;
        existing.Description = module.Description;
        existing.SortOrder = module.SortOrder;
        existing.IsActive = module.IsActive;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Module updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var module = await _context.Modules
            .Include(m => m.RoleModules)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (module == null)
        {
            return NotFound();
        }

        if (module.RoleModules.Any())
        {
            TempData["ErrorMessage"] = "Cannot delete module assigned to roles.";
            return RedirectToAction(nameof(Index));
        }

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Module deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
