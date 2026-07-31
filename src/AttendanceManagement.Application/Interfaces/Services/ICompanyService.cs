using AttendanceManagement.Application.Contracts.Companies;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CompanyDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CompanyDto> CreateAsync(CompanyRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, CompanyRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> HasAnyAsync(CancellationToken cancellationToken = default);
}
