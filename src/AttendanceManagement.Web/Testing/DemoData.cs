using AttendanceManagement.Domain.Entities;
using AttendanceManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AttendanceManagement.Web.Testing;

public static class DemoData
{
    public static async Task SeedAsync(AttendanceDbContext db, IWebHostEnvironment environment, IConfiguration config, Guid? existingCompanyId = null)
    {
        var connection = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        if (!environment.IsDevelopment()) throw new InvalidOperationException("Sample seeding is allowed only in Development.");
        if (existingCompanyId.HasValue)
        {
            if (!string.Equals(connection.DataSource, config["Samples:Server"], StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(connection.InitialCatalog, config["Samples:Database"], StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Explicit sample server and database must match the configured connection.");
        }
        else if (!connection.DataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase) ||
            !connection.InitialCatalog.StartsWith("AttendanceManagement_Demo", StringComparison.Ordinal))
            throw new InvalidOperationException("Demo seeding requires a LocalDB database named AttendanceManagement_Demo*.");
        var password = config["Demo:Password"];
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
            throw new InvalidOperationException("Set Demo__Password to a test password of at least 12 characters.");
        await db.Database.MigrateAsync();
        if (!existingCompanyId.HasValue && await db.Companies.AnyAsync()) return;
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var supervisorNumber = existingCompanyId.HasValue ? config["Samples:SupervisorEmployeeNo"] ?? "DEMO-SUP" : "DEMO-SUP";
        if (supervisorNumber.Length is 0 or > 50) throw new InvalidOperationException("Invalid sample supervisor number.");
        Company company;
        if (existingCompanyId.HasValue)
        {
            company = await db.Companies.SingleOrDefaultAsync(c => c.Id == existingCompanyId && c.IsActive)
                ?? throw new InvalidOperationException("The selected active company was not found.");
            var numbers = new[] { supervisorNumber, "DEMO-001", "DEMO-002", "DEMO-003", "DEMO-OFF" };
            var existing = await db.Employees.Where(e => numbers.Contains(e.EmployeeNo)).ToListAsync();
            if (existing.Count == 5 && existing.All(e => e.CompanyId == company.Id && e.Name.StartsWith("Demo ")) &&
                await db.Groups.AnyAsync(g => g.CompanyId == company.Id && g.Code == "DEMO-ER")) return;
            if (existing.Count != 0 || await db.Groups.AnyAsync(g => g.CompanyId == company.Id && g.Code == "DEMO-ER"))
                throw new InvalidOperationException("Sample identifiers already exist. No existing records will be overwritten.");
        }
        else
        {
            var country = await db.Countries.FirstOrDefaultAsync() ?? new Country { Name = "Pakistan (Demo)" };
            var city = new City { Name = "Karachi (Demo)", Country = country };
            company = new Company { Name = "Demo Hospital - TEST DATA", Domain = "demo.hospital", Country = country, City = city,
                AdminUserId = "demo.admin", AdminPasswordHash = Infrastructure.Services.Security.PasswordHasher.Hash(password) };
            db.Companies.Add(company);
        }
        var category = new Category { Name = "Clinical - Demo" };
        var designation = new Designation { Name = "Nurse - Demo" };
        var supervisorDesignation = new Designation { Name = "Ward Supervisor - Demo", IsIncharge = true };
        var department = new Department { Name = "Emergency - Demo", Company = company };
        var location = new Location { Name = "Demo Main Ward", City = "Karachi", Country = "Pakistan" };
        var policy = new LeavePolicy { Name = "Demo Leave Policy", Company = company };
        var leave = new Leave { Name = "Demo Clinical Leave", Company = company, Category = category, LeavePolicy = policy,
            LeaveDetails = [new() { LeaveType = "Annual", NoOfDays = 20 }, new() { LeaveType = "Sick", NoOfDays = 10 }] };
        var day = new Shift { Company = company, Name = "Demo Day 08-16" };
        var night = new Shift { Company = company, Name = "Demo Night 20-04" };
        foreach (var shift in new[] { day, night })
            foreach (var weekday in Enum.GetValues<DayOfWeek>())
                shift.ShiftDays.Add(new() { DayOfWeek = weekday, IsWorking = weekday != DayOfWeek.Sunday,
                    IsSunday = weekday == DayOfWeek.Sunday, StartTime = TimeSpan.FromHours(shift == day ? 8 : 20), Duration = TimeSpan.FromHours(8) });
        Employee Staff(string number, string name, Shift shift) => new() { Company = company, EmployeeNo = number, Name = name,
            Department = department, Category = category, Designation = designation, Shift = shift, Leave = leave,
            Password = BCrypt.Net.BCrypt.HashPassword(password), JoiningDate = DateTime.Today.AddYears(-1), Email = number + "@example.invalid" };
        var supervisor = Staff(supervisorNumber, "Demo Ward Supervisor", day);
        supervisor.Designation = supervisorDesignation;
        var staff = Staff("DEMO-001", "Demo Nurse One", day); staff.InchargeEmployee = supervisor;
        var nightStaff = Staff("DEMO-002", "Demo Night Nurse", night); nightStaff.InchargeEmployee = supervisor;
        var outsider = Staff("DEMO-003", "Demo Other Team", day);
        var inactive = Staff("DEMO-OFF", "Demo Inactive Employee", day); inactive.IsActive = false;
        db.Employees.AddRange(supervisor, staff, nightStaff, outsider, inactive);
        db.Locations.Add(location);
        var group = new Group { Company = company, Name = "Demo Emergency Team", Code = "DEMO-ER" };
        group.GroupEmployees.Add(new() { Employee = staff });
        group.GroupEmployees.Add(new() { Employee = nightStaff });
        group.GroupDepartments.Add(new() { Department = department });
        group.GroupLocations.Add(new() { Location = location });
        group.GroupIncharges.Add(new() { Employee = supervisor });
        db.Groups.Add(group);
        db.AssignGroups.Add(new() { Company = company, Group = group, Department = department, Location = location, InchargeEmployee = supervisor });
        db.MarkDays.Add(new() { Company = company, Group = group, FromDate = DateTime.Today.AddDays(10), ToDate = DateTime.Today.AddDays(10), TotalDays = 1, IsGazettedHoliday = true });
        await db.SaveChangesAsync();
        var role = new Role { Name = "Demo Employee", DisplayName = "Demo Employee" };
        role.RoleModules.Add(new() { ModuleId = 6, CanView = true });
        db.RoleAssignments.Add(new() { Company = company, Role = role, AssignmentType = "Group", Group = group });
        foreach (var employee in new[] { staff, nightStaff })
        {
            db.EmployeeShiftSchedules.Add(new() { Company = company, Employee = employee, ShiftId = employee.ShiftId!.Value, EffectiveFrom = DateTime.Today.AddMonths(-1) });
            db.EmployeeCalendarDays.Add(new() { Company = company, Employee = employee, Date = DateTime.Today.AddDays(2), DayStatus = "Off", UpdatedByEmployee = supervisor });
            for (var i = 1; i <= 5; i++)
            {
                var punch = DateTime.Today.AddDays(-i).AddHours(employee == staff ? 8 : 20);
                db.Attendances.Add(new() { Company = company, Employee = employee, PunchDateTime = punch, PunchType = "In", DeviceId = "DEMO" });
                db.Attendances.Add(new() { Company = company, Employee = employee, PunchDateTime = punch.AddHours(8), PunchType = "Out", DeviceId = "DEMO" });
            }
        }
        foreach (var status in new[] { "Pending", "Approved", "Rejected", "Cancelled" })
            db.LeaveApplications.Add(new() { Company = company, Employee = staff, Incharge = supervisor, LeaveType = "Annual",
                StartDate = DateTime.Today.AddDays(-20), EndDate = DateTime.Today.AddDays(-20), TotalDays = 1,
                Status = status, InchargeApprovalStatus = status, LeaveNumber = "DEMO-" + status, Reason = "Synthetic test record" });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
