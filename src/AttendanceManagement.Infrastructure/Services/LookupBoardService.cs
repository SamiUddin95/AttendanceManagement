using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Lookups;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

internal class LookupBoardService : ILookupBoardService
{
    private readonly AttendanceDbContext _dbContext;

    public LookupBoardService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetCategoriesAsync(int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Categories.AsNoTracking().OrderBy(c => c.Name);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new LookupEntryDto(c.Id, c.Name, c.IsActive))
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetDesignationsAsync(int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Designations.AsNoTracking().OrderBy(c => c.Name);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new LookupEntryDto(c.Id, c.Name, c.IsActive, c.IsIncharge))
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetDepartmentsAsync(int page = 1, int pageSize = 10)
    {
        var query = _dbContext.Departments.AsNoTracking().OrderBy(c => c.Name);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new LookupEntryDto(c.Id, c.Name, c.IsActive))
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<LookupEntryDto> AddCategoryAsync(string name)
    {
        var category = new Category { Name = name, IsActive = true };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(category.Id, category.Name, category.IsActive);
    }

    public async Task<LookupEntryDto> AddDesignationAsync(string name, bool isIncharge = false)
    {
        var designation = new Designation { Name = name, IsActive = true, IsIncharge = isIncharge };
        _dbContext.Designations.Add(designation);
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(designation.Id, designation.Name, designation.IsActive, designation.IsIncharge);
    }

    public async Task<LookupEntryDto> AddDepartmentAsync(string name)
    {
        var department = new Department { Name = name, IsActive = true };
        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(department.Id, department.Name, department.IsActive);
    }

    public async Task<LookupEntryDto> UpdateCategoryAsync(int id, string name)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return null;
        category.Name = name;
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(category.Id, category.Name, category.IsActive);
    }

    public async Task<LookupEntryDto> UpdateDesignationAsync(int id, string name, bool? isIncharge = null)
    {
        var designation = await _dbContext.Designations.FindAsync(id);
        if (designation == null) return null;
        designation.Name = name;
        if (isIncharge.HasValue)
            designation.IsIncharge = isIncharge.Value;
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(designation.Id, designation.Name, designation.IsActive, designation.IsIncharge);
    }

    public async Task<LookupEntryDto> UpdateDepartmentAsync(int id, string name)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null) return null;
        department.Name = name;
        await _dbContext.SaveChangesAsync();
        return new LookupEntryDto(department.Id, department.Name, department.IsActive);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return false;
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDesignationAsync(int id)
    {
        var designation = await _dbContext.Designations.FindAsync(id);
        if (designation == null) return false;
        _dbContext.Designations.Remove(designation);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null) return false;
        _dbContext.Departments.Remove(department);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleCategoryStatusAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category == null) return false;
        category.IsActive = !category.IsActive;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDesignationStatusAsync(int id)
    {
        var designation = await _dbContext.Designations.FindAsync(id);
        if (designation == null) return false;
        designation.IsActive = !designation.IsActive;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDepartmentStatusAsync(int id)
    {
        var department = await _dbContext.Departments.FindAsync(id);
        if (department == null) return false;
        department.IsActive = !department.IsActive;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleDesignationIsInchargeAsync(int id)
    {
        var designation = await _dbContext.Designations.FindAsync(id);
        if (designation == null) return false;
        designation.IsIncharge = !designation.IsIncharge;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
