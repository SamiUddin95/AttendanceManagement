using AttendanceManagement.Application.Contracts.Companies;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Web.ViewModels.Companies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;
    private readonly ILookupService _lookupService;

    public CompanyController(ICompanyService companyService, ILookupService lookupService)
    {
        _companyService = companyService;
        _lookupService = lookupService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var companies = await _companyService.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(search))
        {
            companies = companies.Where(c => c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var viewModel = new CompanyListViewModel
        {
            Companies = companies,
            Search = search
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = await BuildFormViewModelAsync();
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompanyFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(viewModel);
            return View("Edit", viewModel);
        }

        var request = MapToRequest(viewModel);
        await _companyService.CreateAsync(request);
        TempData["SuccessMessage"] = "Company created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var company = await _companyService.GetByIdAsync(id);
        if (company is null)
        {
            return NotFound();
        }

        var viewModel = await BuildFormViewModelAsync(company);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CompanyFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(viewModel);
            return View(viewModel);
        }

        var request = MapToRequest(viewModel);
        await _companyService.UpdateAsync(id, request);
        TempData["SuccessMessage"] = "Company updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _companyService.DeleteAsync(id);
        TempData["SuccessMessage"] = "Company deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<CompanyFormViewModel> BuildFormViewModelAsync(CompanyDto? company = null)
    {
        var model = new CompanyFormViewModel
        {
            Id = company?.Id,
            Name = company?.Name ?? string.Empty,
            PhoneNumber = company?.PhoneNumber,
            FaxNumber = company?.FaxNumber,
            NumberOfEmployees = company?.NumberOfEmployees,
            AddressLine1 = company?.AddressLine1,
            AddressLine2 = company?.AddressLine2,
            CountryId = company?.CountryId ?? 0,
            CityId = company?.CityId ?? 0,
            Domain = company?.Domain,
            IsActive = company?.IsActive ?? true,
            AdminUserId = company?.AdminUserId ?? string.Empty
        };

        await PopulateLookupsAsync(model);
        return model;
    }

    private async Task PopulateLookupsAsync(CompanyFormViewModel model)
    {
        var countries = await _lookupService.GetCountriesAsync();
        var cities = model.CountryId > 0
            ? await _lookupService.GetCitiesByCountryAsync(model.CountryId)
            : Array.Empty<AttendanceManagement.Application.Contracts.Lookups.CityDto>();

        model.Countries = countries.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CountryId));
        model.Cities = cities.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CityId));
    }

    private static CompanyRequest MapToRequest(CompanyFormViewModel viewModel)
        => new()
        {
            Name = viewModel.Name,
            PhoneNumber = viewModel.PhoneNumber,
            FaxNumber = viewModel.FaxNumber,
            NumberOfEmployees = viewModel.NumberOfEmployees,
            AddressLine1 = viewModel.AddressLine1,
            AddressLine2 = viewModel.AddressLine2,
            CountryId = viewModel.CountryId,
            CityId = viewModel.CityId,
            Domain = viewModel.Domain,
            IsActive = viewModel.IsActive,
            AdminUserId = viewModel.AdminUserId,
            AdminPassword = viewModel.AdminPassword
        };
}
