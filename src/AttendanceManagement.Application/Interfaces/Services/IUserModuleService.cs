using AttendanceManagement.Application.Contracts.Modules;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface IUserModuleService
{
    Task<IReadOnlyList<ModuleDto>> GetUserModulesAsync(Guid companyId, string assignmentType, int? groupId = null, int? departmentId = null, int? employeeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ModuleDto>> GetAllActiveModulesAsync(CancellationToken cancellationToken = default);
}
