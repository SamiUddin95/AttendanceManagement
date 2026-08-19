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
public class RoleController : Controller
{
    private readonly AttendanceDbContext _context;

    public RoleController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var roles = await _context.Roles
            .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
            .OrderBy(r => r.Name)
            .ToListAsync();

        var viewModel = new RoleListViewModel
        {
            Roles = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                DisplayName = r.DisplayName,
                Description = r.Description,
                IsActive = r.IsActive,
                RoleModules = r.RoleModules.Select(rm => new RoleModuleViewModel
                {
                    ModuleId = rm.ModuleId,
                    ModuleName = rm.Module?.Name ?? string.Empty,
                    ModuleDisplayName = rm.Module?.DisplayName ?? string.Empty,
                    CanView = rm.CanView,
                    CanCreate = rm.CanCreate,
                    CanEdit = rm.CanEdit,
                    CanDelete = rm.CanDelete
                }).ToList()
            }).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var modules = await _context.Modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();

        var viewModel = new RoleViewModel
        {
            IsActive = true,
            AvailableModules = modules.Select(m => new ModuleViewModel
            {
                Id = m.Id,
                Name = m.Name,
                DisplayName = m.DisplayName,
                Description = m.Description,
                IsActive = m.IsActive
            }).ToList(),
            RoleModules = modules.Select(m => new RoleModuleViewModel
            {
                ModuleId = m.Id,
                ModuleName = m.Name,
                ModuleDisplayName = m.DisplayName,
                CanView = false,
                CanCreate = false,
                CanEdit = false,
                CanDelete = false
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoleViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateAvailableModules(viewModel);
            return View(viewModel);
        }

        var role = new Role
        {
            Name = viewModel.Name,
            DisplayName = viewModel.DisplayName,
            Description = viewModel.Description,
            IsActive = viewModel.IsActive
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Add role modules
        foreach (var rm in viewModel.RoleModules.Where(m => m.CanView || m.CanCreate || m.CanEdit || m.CanDelete))
        {
            _context.RoleModules.Add(new RoleModule
            {
                RoleId = role.Id,
                ModuleId = rm.ModuleId,
                CanView = rm.CanView,
                CanCreate = rm.CanCreate,
                CanEdit = rm.CanEdit,
                CanDelete = rm.CanDelete
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RoleModules)
                .ThenInclude(rm => rm.Module)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        var allModules = await _context.Modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();

        var viewModel = new RoleViewModel
        {
            Id = role.Id,
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            IsActive = role.IsActive,
            AvailableModules = allModules.Select(m => new ModuleViewModel
            {
                Id = m.Id,
                Name = m.Name,
                DisplayName = m.DisplayName,
                Description = m.Description,
                IsActive = m.IsActive
            }).ToList(),
            RoleModules = allModules.Select(m =>
            {
                var existing = role.RoleModules.FirstOrDefault(rm => rm.ModuleId == m.Id);
                return new RoleModuleViewModel
                {
                    ModuleId = m.Id,
                    ModuleName = m.Name,
                    ModuleDisplayName = m.DisplayName,
                    CanView = existing?.CanView ?? false,
                    CanCreate = existing?.CanCreate ?? false,
                    CanEdit = existing?.CanEdit ?? false,
                    CanDelete = existing?.CanDelete ?? false
                };
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RoleViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateAvailableModules(viewModel);
            return View(viewModel);
        }

        var role = await _context.Roles
            .Include(r => r.RoleModules)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        role.Name = viewModel.Name;
        role.DisplayName = viewModel.DisplayName;
        role.Description = viewModel.Description;
        role.IsActive = viewModel.IsActive;
        role.UpdatedAtUtc = DateTime.UtcNow;

        // Update role modules
        var existingModuleIds = role.RoleModules.Select(rm => rm.ModuleId).ToList();
        var submittedModuleIds = viewModel.RoleModules
            .Where(m => m.CanView || m.CanCreate || m.CanEdit || m.CanDelete)
            .Select(m => m.ModuleId)
            .ToList();

        // Remove modules not in submitted list
        var toRemove = role.RoleModules.Where(rm => !submittedModuleIds.Contains(rm.ModuleId)).ToList();
        _context.RoleModules.RemoveRange(toRemove);

        // Update or add modules
        foreach (var rm in viewModel.RoleModules.Where(m => m.CanView || m.CanCreate || m.CanEdit || m.CanDelete))
        {
            var existing = role.RoleModules.FirstOrDefault(e => e.ModuleId == rm.ModuleId);
            if (existing != null)
            {
                existing.CanView = rm.CanView;
                existing.CanCreate = rm.CanCreate;
                existing.CanEdit = rm.CanEdit;
                existing.CanDelete = rm.CanDelete;
            }
            else
            {
                role.RoleModules.Add(new RoleModule
                {
                    RoleId = role.Id,
                    ModuleId = rm.ModuleId,
                    CanView = rm.CanView,
                    CanCreate = rm.CanCreate,
                    CanEdit = rm.CanEdit,
                    CanDelete = rm.CanDelete
                });
            }
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var role = await _context.Roles
            .Include(r => r.RoleModules)
            .Include(r => r.RoleAssignments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role == null)
        {
            return NotFound();
        }

        // Check if role has assignments
        if (role.RoleAssignments.Any())
        {
            TempData["ErrorMessage"] = "Cannot delete role with existing assignments.";
            return RedirectToAction(nameof(Index));
        }

        _context.RoleModules.RemoveRange(role.RoleModules);
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Role deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateAvailableModules(RoleViewModel viewModel)
    {
        var modules = await _context.Modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .ToListAsync();

        viewModel.AvailableModules = modules.Select(m => new ModuleViewModel
        {
            Id = m.Id,
            Name = m.Name,
            DisplayName = m.DisplayName,
            Description = m.Description,
            IsActive = m.IsActive
        }).ToList();

        // Ensure RoleModules has entries for all available modules
        var existingModuleIds = viewModel.RoleModules.Select(m => m.ModuleId).ToHashSet();
        foreach (var module in modules)
        {
            if (!existingModuleIds.Contains(module.Id))
            {
                viewModel.RoleModules.Add(new RoleModuleViewModel
                {
                    ModuleId = module.Id,
                    ModuleName = module.Name,
                    ModuleDisplayName = module.DisplayName,
                    CanView = true,
                    CanCreate = false,
                    CanEdit = false,
                    CanDelete = false
                });
            }
        }
    }
}
