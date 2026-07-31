namespace AttendanceManagement.Application.Contracts.Lookups;

public record LookupEntryDto(int Id, string Name, bool IsActive, bool? IsIncharge = null);
