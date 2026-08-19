using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AttendanceManagement.Application.Contracts.Modules;
using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

internal class UserModuleService : IUserModuleService
{
    private readonly AttendanceDbContext _dbContext;

    public UserModuleService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ModuleDto>> GetUserModulesAsync(Guid companyId, string assignmentType, int? groupId = null, int? departmentId = null, int? employeeId = null, CancellationToken cancellationToken = default)
    {
        // Build base query with includes
        IQueryable<RoleAssignment> query = _dbContext.RoleAssignments
            .Where(ra => ra.CompanyId == companyId && ra.IsActive)
            .Include(ra => ra.Role)
                .ThenInclude(r => r.RoleModules)
                    .ThenInclude(rm => rm.Module);

        // Apply assignment type filter
        if (assignmentType == "Group")
        {
            query = query.Where(ra => ra.AssignmentType == "Group" && ra.GroupId == groupId);
        }
        else if (assignmentType == "Individual")
        {
            query = query.Where(ra => ra.AssignmentType == "Individual" && ra.EmployeeId == employeeId);
        }
        else
        {
            return Array.Empty<ModuleDto>();
        }

        var assignments = await query.ToListAsync(cancellationToken);

        // Collect all modules from assigned roles where CanView is true
        var moduleIds = assignments
            .SelectMany(ra => ra.Role?.RoleModules ?? Enumerable.Empty<RoleModule>())
            .Where(rm => rm.CanView && rm.Module?.IsActive == true)
            .Select(rm => rm.ModuleId)
            .Distinct()
            .ToList();

        if (!moduleIds.Any())
        {
            return Array.Empty<ModuleDto>();
        }

        var modules = await _dbContext.Modules
            .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .Select(m => new ModuleDto
            {
                Id = m.Id,
                Name = m.Name,
                DisplayName = m.DisplayName,
                Description = m.Description,
                Area = m.Area,
                Controller = m.Controller,
                Action = m.Action,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                IsActive = m.IsActive
            })
            .ToListAsync(cancellationToken);

        return modules;
    }

    public async Task<IReadOnlyList<ModuleDto>> GetAllActiveModulesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Modules
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Name)
            .Select(m => new ModuleDto
            {
                Id = m.Id,
                Name = m.Name,
                DisplayName = m.DisplayName,
                Description = m.Description,
                Area = m.Area,
                Controller = m.Controller,
                Action = m.Action,
                Icon = m.Icon,
                SortOrder = m.SortOrder,
                IsActive = m.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
