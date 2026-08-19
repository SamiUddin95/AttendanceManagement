using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using AttendanceManagement.Web.ViewModels.Employees;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Areas.Administrator.Controllers;

[Area("Administrator")]
[Authorize]
public class EmployeeController : Controller
{
    private readonly AttendanceDbContext _context;

    public EmployeeController(AttendanceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Department)
            .Include(e => e.Category)
            .Include(e => e.Designation)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.Name.Contains(search) ||
                                     e.EmployeeNo.Contains(search));
        }

        var employees = await query.ToListAsync();
        var viewModels = employees.Select(e => new EmployeeViewModel
        {
            Id = e.Id,
            Name = e.Name,
            CompanyId = e.CompanyId,
            CompanyName = e.Company?.Name,
            EmployeeType = e.EmployeeType,
            DepartmentId = e.DepartmentId,
            DepartmentName = e.Department?.Name,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name,
            DesignationId = e.DesignationId,
            DesignationName = e.Designation?.Name,
            ShiftId = e.ShiftId,
            LeaveId = e.LeaveId,
            JoiningDate = e.JoiningDate,
            EmployeeNo = e.EmployeeNo,
            Cell = e.Cell,
            Email = e.Email,
            IsActive = e.IsActive
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

        var viewModel = new EmployeeViewModel
        {
            CompanyId = companyId,
            IsActive = true,
            OverTimeEntitled = false,
            EmployeeType = "Permanent"
        };
        ViewBag.CompanyId = companyId;
        ViewBag.CompanyName = companyName;
        await PopulateLookupsAsync(viewModel);
        return View("Edit", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(viewModel);
            return View("Edit", viewModel);
        }

        var employee = new Employee
        {
            Name = viewModel.Name,
            CompanyId = viewModel.CompanyId,
            EmployeeType = viewModel.EmployeeType,
            DepartmentId = viewModel.DepartmentId,
            CategoryId = viewModel.CategoryId,
            DesignationId = viewModel.DesignationId,
            ShiftId = viewModel.ShiftId,
            LeaveId = viewModel.LeaveId,
            JoiningDate = viewModel.JoiningDate,
            PeriodEndDate = viewModel.PeriodEndDate,
            ResignationDate = viewModel.ResignationDate,
            EfficiencyRequired = viewModel.EfficiencyRequired,
            InchargeCategoryId = viewModel.InchargeCategoryId,
            InchargeDesignationId = viewModel.InchargeDesignationId,
            InchargeEmployeeId = viewModel.InchargeEmployeeId,
            ImageBase64 = viewModel.ImageBase64,
            EmployeeNo = viewModel.EmployeeNo,
            CardNo = viewModel.CardNo,
            FatherSpouseName = viewModel.FatherSpouseName,
            Channel = viewModel.Channel,
            BloodGroup = viewModel.BloodGroup,
            Address = viewModel.Address,
            Phone = viewModel.Phone,
            Cell = viewModel.Cell,
            Email = viewModel.Email,
            Password = !string.IsNullOrWhiteSpace(viewModel.Password) ? BCrypt.Net.BCrypt.HashPassword(viewModel.Password) : null,
            Gender = viewModel.Gender,
            DateOfBirth = viewModel.DateOfBirth,
            CNIC = viewModel.CNIC,
            OverTimeEntitled = viewModel.OverTimeEntitled,
            IsActive = viewModel.IsActive
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Employee created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Department)
            .Include(e => e.Category)
            .Include(e => e.Designation)
            .Include(e => e.Shift)
            .Include(e => e.Leave)
            .Include(e => e.InchargeCategory)
            .Include(e => e.InchargeDesignation)
            .Include(e => e.InchargeEmployee)
            .FirstOrDefaultAsync(e => e.Id == id);
        
        if (employee is null)
        {
            return NotFound();
        }

        var viewModel = new EmployeeViewModel
        {
            Id = employee.Id,
            Name = employee.Name,
            CompanyId = employee.CompanyId,
            CompanyName = employee.Company?.Name,
            EmployeeType = employee.EmployeeType,
            DepartmentId = employee.DepartmentId,
            DepartmentName = employee.Department?.Name,
            CategoryId = employee.CategoryId,
            CategoryName = employee.Category?.Name,
            DesignationId = employee.DesignationId,
            DesignationName = employee.Designation?.Name,
            ShiftId = employee.ShiftId,
            ShiftName = employee.Shift?.Name,
            LeaveId = employee.LeaveId,
            LeaveName = employee.Leave?.Name,
            JoiningDate = employee.JoiningDate,
            PeriodEndDate = employee.PeriodEndDate,
            ResignationDate = employee.ResignationDate,
            EfficiencyRequired = employee.EfficiencyRequired,
            InchargeCategoryId = employee.InchargeCategoryId,
            InchargeCategoryName = employee.InchargeCategory?.Name,
            InchargeDesignationId = employee.InchargeDesignationId,
            InchargeDesignationName = employee.InchargeDesignation?.Name,
            InchargeEmployeeId = employee.InchargeEmployeeId,
            InchargeEmployeeName = employee.InchargeEmployee?.Name,
            ImageBase64 = employee.ImageBase64,
            EmployeeNo = employee.EmployeeNo,
            CardNo = employee.CardNo,
            FatherSpouseName = employee.FatherSpouseName,
            Channel = employee.Channel,
            BloodGroup = employee.BloodGroup,
            Address = employee.Address,
            Phone = employee.Phone,
            Cell = employee.Cell,
            Email = employee.Email,
            Password = employee.Password,
            Gender = employee.Gender,
            DateOfBirth = employee.DateOfBirth,
            CNIC = employee.CNIC,
            OverTimeEntitled = employee.OverTimeEntitled,
            IsActive = employee.IsActive
        };

        ViewBag.CompanyId = employee.CompanyId;
        ViewBag.CompanyName = employee.Company?.Name;
        await PopulateLookupsAsync(viewModel);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmployeeViewModel viewModel)
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

        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        employee.Name = viewModel.Name;
        employee.EmployeeType = viewModel.EmployeeType;
        employee.DepartmentId = viewModel.DepartmentId;
        employee.CategoryId = viewModel.CategoryId;
        employee.DesignationId = viewModel.DesignationId;
        employee.ShiftId = viewModel.ShiftId;
        employee.LeaveId = viewModel.LeaveId;
        employee.JoiningDate = viewModel.JoiningDate;
        employee.PeriodEndDate = viewModel.PeriodEndDate;
        employee.ResignationDate = viewModel.ResignationDate;
        employee.EfficiencyRequired = viewModel.EfficiencyRequired;
        employee.InchargeCategoryId = viewModel.InchargeCategoryId;
        employee.InchargeDesignationId = viewModel.InchargeDesignationId;
        employee.InchargeEmployeeId = viewModel.InchargeEmployeeId;
        employee.ImageBase64 = viewModel.ImageBase64;
        employee.EmployeeNo = viewModel.EmployeeNo;
        employee.CardNo = viewModel.CardNo;
        employee.FatherSpouseName = viewModel.FatherSpouseName;
        employee.Channel = viewModel.Channel;
        employee.BloodGroup = viewModel.BloodGroup;
        employee.Address = viewModel.Address;
        employee.Phone = viewModel.Phone;
        employee.Cell = viewModel.Cell;
        employee.Email = viewModel.Email;
        if (!string.IsNullOrWhiteSpace(viewModel.Password))
        {
            employee.Password = BCrypt.Net.BCrypt.HashPassword(viewModel.Password);
        }
        employee.Gender = viewModel.Gender;
        employee.DateOfBirth = viewModel.DateOfBirth;
        employee.CNIC = viewModel.CNIC;
        employee.OverTimeEntitled = viewModel.OverTimeEntitled;
        employee.IsActive = viewModel.IsActive;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Employee updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Employee deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(EmployeeViewModel viewModel)
    {
        var departments = await _context.Departments.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();
        var categories = await _context.Categories.Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        var designations = await _context.Designations.Where(d => d.IsActive).OrderBy(d => d.Name).ToListAsync();
        var shifts = await _context.Shifts.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
        var leaves = await _context.Leaves.Where(l => l.IsActive).OrderBy(l => l.Name).ToListAsync();
        var employees = await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();

        viewModel.Departments = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == viewModel.DepartmentId)).ToList();
        viewModel.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == viewModel.CategoryId)).ToList();
        viewModel.Designations = designations.Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == viewModel.DesignationId)).ToList();
        viewModel.Shifts = shifts.Select(s => new SelectListItem(s.Name, s.Id.ToString(), s.Id == viewModel.ShiftId)).ToList();
        viewModel.Leaves = leaves.Select(l => new SelectListItem(l.Name, l.Id.ToString(), l.Id == viewModel.LeaveId)).ToList();
        viewModel.InchargeCategories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString(), c.Id == viewModel.InchargeCategoryId)).ToList();
        viewModel.InchargeDesignations = designations.Select(d => new SelectListItem(d.Name, d.Id.ToString(), d.Id == viewModel.InchargeDesignationId)).ToList();
        viewModel.InchargeEmployees = employees.Select(e => new SelectListItem(e.Name, e.Id.ToString(), e.Id == viewModel.InchargeEmployeeId)).ToList();

        // Employee types
        var employeeTypes = new[] { "Permanent", "Contract", "Part-Time", "Temporary/Seasonal", "Intern" };
        viewModel.EmployeeTypes = employeeTypes.Select(et => new SelectListItem(et, et, et == viewModel.EmployeeType)).ToList();

        // Blood groups
        var bloodGroups = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
        viewModel.BloodGroups = bloodGroups.Select(bg => new SelectListItem(bg, bg, bg == viewModel.BloodGroup)).ToList();

        // Genders
        var genders = new[] { "Male", "Female", "Other" };
        viewModel.Genders = genders.Select(g => new SelectListItem(g, g, g == viewModel.Gender)).ToList();

        // Efficiency options (percentages)
        var efficiencyOptions = new[] { "50%", "60%", "70%", "80%", "90%", "95%", "100%" };
        viewModel.EfficiencyOptions = efficiencyOptions.Select(eo => new SelectListItem(eo, eo, eo == viewModel.EfficiencyRequired)).ToList();
    }
}
