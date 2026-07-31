using System.Collections.Generic;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Lookups;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface ILookupBoardService
{
    Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetCategoriesAsync(int page = 1, int pageSize = 10);
    Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetDesignationsAsync(int page = 1, int pageSize = 10);
    Task<(IReadOnlyList<LookupEntryDto> Items, int TotalCount)> GetDepartmentsAsync(int page = 1, int pageSize = 10);

    Task<LookupEntryDto> AddCategoryAsync(string name);
    Task<LookupEntryDto> AddDesignationAsync(string name, bool isIncharge = false);
    Task<LookupEntryDto> AddDepartmentAsync(string name);

    Task<LookupEntryDto> UpdateCategoryAsync(int id, string name);
    Task<LookupEntryDto> UpdateDesignationAsync(int id, string name, bool? isIncharge = null);
    Task<LookupEntryDto> UpdateDepartmentAsync(int id, string name);

    Task<bool> DeleteCategoryAsync(int id);
    Task<bool> DeleteDesignationAsync(int id);
    Task<bool> DeleteDepartmentAsync(int id);

    Task<bool> ToggleCategoryStatusAsync(int id);
    Task<bool> ToggleDesignationStatusAsync(int id);
    Task<bool> ToggleDepartmentStatusAsync(int id);

    Task<bool> ToggleDesignationIsInchargeAsync(int id);
}
