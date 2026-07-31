using AttendanceManagement.Application.Interfaces.Services;
using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly AttendanceDbContext _dbContext;

    public ShiftService(AttendanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Shift> Items, int TotalCount)> GetShiftsAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        var query = _dbContext.Shifts.Include(s => s.Company).Include(s => s.ShiftDays).AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.Contains(search) || 
                                     s.Company.Name.Contains(search) ||
                                     (s.Description != null && s.Description.Contains(search)));
        }
        
        query = query.OrderBy(s => s.Name);
        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, totalCount);
    }

    public async Task<Shift?> GetShiftByIdAsync(int id)
    {
        return await _dbContext.Shifts
            .Include(s => s.Company)
            .Include(s => s.ShiftDays.OrderBy(d => d.DayOfWeek))
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Shift> CreateShiftAsync(Shift shift)
    {
        _dbContext.Shifts.Add(shift);
        await _dbContext.SaveChangesAsync();
        return shift;
    }

    public async Task<Shift?> UpdateShiftAsync(Shift shift)
    {
        var existing = await _dbContext.Shifts
            .Include(s => s.ShiftDays)
            .FirstOrDefaultAsync(s => s.Id == shift.Id);
        
        if (existing == null) return null;

        existing.Name = shift.Name;
        existing.Description = shift.Description;
        existing.IsActive = shift.IsActive;
        existing.CompanyId = shift.CompanyId;

        // Handle ShiftDays
        var existingDayIds = existing.ShiftDays.Select(d => d.Id).ToList();
        var incomingDayIds = shift.ShiftDays.Where(d => d.Id > 0).Select(d => d.Id).ToList();

        // Remove deleted days
        var daysToDelete = existing.ShiftDays.Where(d => !incomingDayIds.Contains(d.Id)).ToList();
        _dbContext.ShiftDays.RemoveRange(daysToDelete);

        // Update or add days
        foreach (var day in shift.ShiftDays)
        {
            if (day.Id > 0)
            {
                var existingDay = existing.ShiftDays.FirstOrDefault(d => d.Id == day.Id);
                if (existingDay != null)
                {
                    existingDay.DayOfWeek = day.DayOfWeek;
                    existingDay.IsWorking = day.IsWorking;
                    existingDay.IsAlternate = day.IsAlternate;
                    existingDay.IsHoliday = day.IsHoliday;
                    existingDay.IsOther = day.IsOther;
                    existingDay.OtherHours = day.OtherHours;
                    existingDay.OtherMinutes = day.OtherMinutes;
                    existingDay.StartTime = day.StartTime;
                    existingDay.Duration = day.Duration;
                    existingDay.LunchBreak = day.LunchBreak;
                    existingDay.PrayerBreak = day.PrayerBreak;
                    existingDay.TeaBreak = day.TeaBreak;
                    existingDay.FlexiIn = day.FlexiIn;
                    existingDay.FlexiOut = day.FlexiOut;
                    existingDay.IsFlexible = day.IsFlexible;
                    existingDay.CopyFromPrevious = day.CopyFromPrevious;
                }
            }
            else
            {
                day.ShiftId = existing.Id;
                existing.ShiftDays.Add(day);
            }
        }

        await _dbContext.SaveChangesAsync();
        return await GetShiftByIdAsync(shift.Id);
    }

    public async Task<bool> DeleteShiftAsync(int id)
    {
        var shift = await _dbContext.Shifts.FindAsync(id);
        if (shift == null) return false;
        _dbContext.Shifts.Remove(shift);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleShiftStatusAsync(int id)
    {
        var shift = await _dbContext.Shifts.FindAsync(id);
        if (shift == null) return false;
        shift.IsActive = !shift.IsActive;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
