using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Locations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class LocationController : Controller
{
    private readonly AttendanceDbContext _context;

    public LocationController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Locations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(l => l.Name.ToLower().Contains(searchLower) ||
                                     l.Address != null && l.Address.ToLower().Contains(searchLower) ||
                                     l.City != null && l.City.ToLower().Contains(searchLower) ||
                                     l.Country != null && l.Country.ToLower().Contains(searchLower));
        }

        var locations = await query.OrderByDescending(l => l.CreatedAtUtc).ToListAsync();
        var viewModels = locations.Select(l => new LocationViewModel
        {
            Id = l.Id,
            Name = l.Name,
            Address = l.Address,
            City = l.City,
            Country = l.Country,
            PostalCode = l.PostalCode,
            PhoneNumber = l.PhoneNumber,
            IsActive = l.IsActive
        }).ToList();

        var listViewModel = new LocationListViewModel
        {
            Locations = viewModels,
            Search = search
        };

        return View(listViewModel);
    }

    public IActionResult Create()
    {
        return View("Edit", new LocationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LocationViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", viewModel);
        }

        var location = new Location
        {
            Name = viewModel.Name,
            Address = viewModel.Address,
            City = viewModel.City,
            Country = viewModel.Country,
            PostalCode = viewModel.PostalCode,
            PhoneNumber = viewModel.PhoneNumber,
            IsActive = viewModel.IsActive
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Location created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null)
        {
            return NotFound();
        }

        var viewModel = new LocationViewModel
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            Country = location.Country,
            PostalCode = location.PostalCode,
            PhoneNumber = location.PhoneNumber,
            IsActive = location.IsActive
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LocationViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var location = await _context.Locations.FindAsync(id);
        if (location is null)
        {
            return NotFound();
        }

        location.Name = viewModel.Name;
        location.Address = viewModel.Address;
        location.City = viewModel.City;
        location.Country = viewModel.Country;
        location.PostalCode = viewModel.PostalCode;
        location.PhoneNumber = viewModel.PhoneNumber;
        location.IsActive = viewModel.IsActive;

        _context.Locations.Update(location);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Location updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location is null)
        {
            return NotFound();
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Location deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
