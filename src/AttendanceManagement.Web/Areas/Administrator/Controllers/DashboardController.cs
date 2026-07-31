using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Lookups;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Web.ViewModels.Lookups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class DashboardController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ILookupBoardService _lookupBoardService;

    public DashboardController(ICompanyService companyService, ILookupBoardService lookupBoardService)
    {
        _companyService = companyService;
        _lookupBoardService = lookupBoardService;
    }

    public async Task<IActionResult> Index()
    {
        var companies = await _companyService.GetAllAsync();
        var total = companies.Count();
        var active = companies.Count(c => c.IsActive);
        var inactive = total - active;

        ViewData["TotalCompanies"] = total;
        ViewData["ActiveCompanies"] = active;
        ViewData["InactiveCompanies"] = inactive;

        return View(companies.Take(6));
    }

    public async Task<IActionResult> Lookups(string? section = null, int? categoriesPage = null, int? designationsPage = null, int? departmentsPage = null)
    {
        var pageSize = 10;

        var sections = new List<LookupSectionViewModel>
        {
            await BuildSectionAsync("categories", "Categories", "Control the categories used for jobs and reporting.", _lookupBoardService.GetCategoriesAsync, categoriesPage ?? 1, pageSize),
            await BuildSectionAsync("designations", "Designation", "Titles that appear on employee profiles and letters.", _lookupBoardService.GetDesignationsAsync, designationsPage ?? 1, pageSize),
            await BuildSectionAsync("departments", "Department", "Business units every employee belongs to.", _lookupBoardService.GetDepartmentsAsync, departmentsPage ?? 1, pageSize)
        };

        var viewModel = new LookupBoardViewModel
        {
            ActiveSection = section ?? "categories",
            Sections = sections
        };

        ViewData["Title"] = "Lookup Catalog";
        return View("Lookups", viewModel);
    }

    private static async Task<LookupSectionViewModel> BuildSectionAsync(
        string key,
        string title,
        string description,
        Func<int, int, Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)>> factory,
        int currentPage,
        int pageSize)
    {
        var (items, totalCount) = await factory(currentPage, pageSize);
        return new LookupSectionViewModel
        {
            Key = key,
            Title = title,
            Description = description,
            Items = items.Select(i => new LookupItemViewModel
            {
                Id = i.Id,
                Name = i.Name,
                IsActive = i.IsActive,
                IsIncharge = i.IsIncharge
            }).ToList(),
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.AddCategoryAsync(name);
        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDesignation(string name, bool isIncharge = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.AddDesignationAsync(name, isIncharge);
        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddDepartment(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.AddDepartmentAsync(name);
        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCategory(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.UpdateCategoryAsync(id, name);
        if (result == null)
            return Json(new { success = false, message = "Item not found" });

        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDesignation(int id, string name, bool? isIncharge = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.UpdateDesignationAsync(id, name, isIncharge);
        if (result == null)
            return Json(new { success = false, message = "Item not found" });

        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDepartment(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Json(new { success = false, message = "Name is required" });

        var result = await _lookupBoardService.UpdateDepartmentAsync(id, name);
        if (result == null)
            return Json(new { success = false, message = "Item not found" });

        return Json(new { success = true, data = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _lookupBoardService.DeleteCategoryAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDesignation(int id)
    {
        var result = await _lookupBoardService.DeleteDesignationAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await _lookupBoardService.DeleteDepartmentAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCategoryStatus(int id)
    {
        var result = await _lookupBoardService.ToggleCategoryStatusAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDesignationStatus(int id)
    {
        var result = await _lookupBoardService.ToggleDesignationStatusAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDepartmentStatus(int id)
    {
        var result = await _lookupBoardService.ToggleDepartmentStatusAsync(id);
        return Json(new { success = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDesignationIsIncharge(int id)
    {
        var result = await _lookupBoardService.ToggleDesignationIsInchargeAsync(id);
        return Json(new { success = result });
    }
}
