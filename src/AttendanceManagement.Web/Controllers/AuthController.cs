using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Auth;
using AttendanceManagement.Application.Contracts.Companies;
using AttendanceManagement.Application.Contracts.Lookups;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Auth;
using AttendanceManagement.Web.ViewModels.Companies;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ICompanyService _companyService;
    private readonly ILookupService _lookupService;
    private readonly AttendanceDbContext _context;

    public AuthController(IAuthService authService, ICompanyService companyService, ILookupService lookupService, AttendanceDbContext context)
    {
        _authService = authService;
        _companyService = companyService;
        _lookupService = lookupService;
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Administrator" });
        }

        ViewData["ReturnUrl"] = returnUrl;
        var landingViewModel = await BuildLandingViewModelAsync(returnUrl);
        return View(landingViewModel);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login([Bind(Prefix = "Login")] AdminLoginViewModel viewModel, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            var invalidLandingModel = await BuildLandingViewModelAsync(returnUrl, viewModel);
            return View(invalidLandingModel);
        }

        var request = new AdminLoginRequest
        {
            CompanyIdentifier = viewModel.CompanyIdentifier,
            AdminUserId = viewModel.AdminUserId,
            Password = viewModel.Password
        };

        var company = await _authService.LoginAsync(request);
        if (company is null)
        {
            viewModel.ErrorMessage = "Invalid credentials or inactive company.";
            var invalidLandingModel = await BuildLandingViewModelAsync(returnUrl, viewModel);
            return View(invalidLandingModel);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, company.Id.ToString()),
            new(ClaimTypes.Name, company.Name),
            new(ClaimTypes.Role, "SuperUser"), // Company admins get SuperUser role
            new("CompanyDomain", company.Domain ?? string.Empty),
            new("CompanyId", company.Id.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard", new { area = "Administrator" });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupCompany([Bind(Prefix = "Company")] CompanyFormViewModel viewModel)
    {
        if (await _companyService.HasAnyAsync())
        {
            TempData["InfoMessage"] = "Company setup is already complete. Please log in.";
            return RedirectToAction(nameof(Login));
        }

        if (string.IsNullOrWhiteSpace(viewModel.AdminPassword))
        {
            ModelState.AddModelError("Company.AdminPassword", "Admin Password is required.");
        }

        if (!ModelState.IsValid)
        {
            var invalidLandingModel = await BuildLandingViewModelAsync(null, null, viewModel);
            return View("Login", invalidLandingModel);
        }

        var request = MapToRequest(viewModel);
        await _companyService.CreateAsync(request);
        TempData["SuccessMessage"] = "Company created successfully. You can now log in.";
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Cities(int countryId)
    {
        var cities = await _lookupService.GetCitiesByCountryAsync(countryId);
        var response = cities.Select(city => new { city.Id, city.Name });
        return Json(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EmployeeLogin([Bind(Prefix = "EmployeeLogin")] EmployeeLoginViewModel viewModel, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            var invalidLandingModel = await BuildLandingViewModelAsync(returnUrl, null, null, viewModel);
            return View("Login", invalidLandingModel);
        }

        var employee = await _context.Employees
            .Include(e => e.Company)
            .FirstOrDefaultAsync(e => e.EmployeeNo == viewModel.EmployeeNo && e.IsActive);

        if (employee == null || string.IsNullOrWhiteSpace(employee.Password) || !BCrypt.Net.BCrypt.Verify(viewModel.Password, employee.Password))
        {
            viewModel.ErrorMessage = "Invalid Employee No or Password.";
            var invalidLandingModel = await BuildLandingViewModelAsync(returnUrl, null, null, viewModel);
            return View("Login", invalidLandingModel);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, employee.EmployeeNo), // Use EmployeeNo for User.Identity.Name
            new(ClaimTypes.GivenName, employee.Name), // Store actual name in GivenName
            new(ClaimTypes.Role, "Administrator"), // Employees get Administrator role for SuperUser access
            new("EmployeeNo", employee.EmployeeNo),
            new("CompanyId", employee.CompanyId.ToString()),
            new("IsEmployee", "true")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Dashboard", new { area = "Administrator" });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task<AuthLandingViewModel> BuildLandingViewModelAsync(string? returnUrl = null,
        AdminLoginViewModel? loginOverride = null,
        CompanyFormViewModel? companyOverride = null,
        EmployeeLoginViewModel? employeeLoginOverride = null)
    {
        var hasAnyCompany = await _companyService.HasAnyAsync();
        var viewModel = new AuthLandingViewModel
        {
            HasAnyCompany = hasAnyCompany,
            Login = loginOverride ?? new AdminLoginViewModel(),
            EmployeeLogin = employeeLoginOverride ?? new EmployeeLoginViewModel(),
            Company = companyOverride ?? new CompanyFormViewModel(),
            ReturnUrl = returnUrl
        };

        if (!hasAnyCompany)
        {
            await PopulateCompanyLookupsAsync(viewModel.Company);
        }

        return viewModel;
    }

    private async Task PopulateCompanyLookupsAsync(CompanyFormViewModel model)
    {
        var countries = (await _lookupService.GetCountriesAsync()).ToList();
        if (model.CountryId <= 0)
        {
            model.CountryId = countries.FirstOrDefault()?.Id ?? 0;
        }

        var cities = model.CountryId > 0
            ? (await _lookupService.GetCitiesByCountryAsync(model.CountryId)).ToList()
            : new List<CityDto>();

        if (model.CityId <= 0)
        {
            model.CityId = cities.FirstOrDefault()?.Id ?? 0;
        }

        model.Countries = countries
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CountryId));
        model.Cities = cities
            .Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == model.CityId));
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
