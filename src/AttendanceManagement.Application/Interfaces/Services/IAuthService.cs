using AttendanceManagement.Application.Contracts.Auth;
using AttendanceManagement.Application.Contracts.Companies;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface IAuthService
{
    Task<CompanyDto?> LoginAsync(AdminLoginRequest request, CancellationToken cancellationToken = default);
}
