using AttendanceManagement.Domain.Entities;

namespace AttendanceManagement.Application.Interfaces.Services;

public interface IShiftService
{
    Task<(IReadOnlyList<Shift> Items, int TotalCount)> GetShiftsAsync(int page = 1, int pageSize = 10, string? search = null);
    Task<Shift?> GetShiftByIdAsync(int id);
    Task<Shift> CreateShiftAsync(Shift shift);
    Task<Shift?> UpdateShiftAsync(Shift shift);
    Task<bool> DeleteShiftAsync(int id);
    Task<bool> ToggleShiftStatusAsync(int id);
}
