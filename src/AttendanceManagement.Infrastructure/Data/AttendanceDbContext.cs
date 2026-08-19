using AttendanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AttendanceManagement.Infrastructure.Data;

public class AttendanceDbContext : DbContext
{
    public AttendanceDbContext(DbContextOptions<AttendanceDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftDay> ShiftDays => Set<ShiftDay>();
    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<LeaveDetail> LeaveDetails => Set<LeaveDetail>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupDepartment> GroupDepartments => Set<GroupDepartment>();
    public DbSet<GroupLocation> GroupLocations => Set<GroupLocation>();
    public DbSet<GroupDesignation> GroupDesignations => Set<GroupDesignation>();
    public DbSet<GroupIncharge> GroupIncharges => Set<GroupIncharge>();
    public DbSet<AssignGroup> AssignGroups => Set<AssignGroup>();
    public DbSet<LeaveApplication> LeaveApplications => Set<LeaveApplication>();
    public DbSet<MarkDay> MarkDays => Set<MarkDay>();
    public DbSet<GroupEmployee> GroupEmployees => Set<GroupEmployee>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleModule> RoleModules => Set<RoleModule>();
    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasIndex(e => e.Name).IsUnique(false);
            entity.Property(e => e.AdminPasswordHash).IsRequired();
            entity.Property(e => e.AdminUserId).HasMaxLength(100);
            entity.HasOne(e => e.Country)
                .WithMany(c => c.Companies)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.City)
                .WithMany(c => c.Companies)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("Cities");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.HasOne(e => e.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.ToTable("Designations");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.ToTable("Shifts");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShiftDay>(entity =>
        {
            entity.ToTable("ShiftDays");
            entity.HasOne(e => e.Shift)
                .WithMany(s => s.ShiftDays)
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LeavePolicy>(entity =>
        {
            entity.ToTable("LeavePolicies");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Active");
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Leave>(entity =>
        {
            entity.ToTable("Leaves");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.LeavePolicy)
                .WithMany()
                .HasForeignKey(e => e.LeavePolicyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveDetail>(entity =>
        {
            entity.ToTable("LeaveDetails");
            entity.Property(e => e.LeaveType).HasMaxLength(100);
            entity.HasOne(e => e.Leave)
                .WithMany(l => l.LeaveDetails)
                .HasForeignKey(e => e.LeaveId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.EmployeeType).HasMaxLength(50).HasDefaultValue("Permanent");
            entity.Property(e => e.EfficiencyRequired).HasMaxLength(10);
            entity.Property(e => e.EmployeeNo).HasMaxLength(50);
            entity.Property(e => e.CardNo).HasMaxLength(50);
            entity.Property(e => e.FatherSpouseName).HasMaxLength(150);
            entity.Property(e => e.Channel).HasMaxLength(100);
            entity.Property(e => e.BloodGroup).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Cell).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.CNIC).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.OverTimeEntitled).HasDefaultValue(false);

            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Designation)
                .WithMany()
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Shift)
                .WithMany()
                .HasForeignKey(e => e.ShiftId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Leave)
                .WithMany()
                .HasForeignKey(e => e.LeaveId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeCategory)
                .WithMany()
                .HasForeignKey(e => e.InchargeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeDesignation)
                .WithMany()
                .HasForeignKey(e => e.InchargeDesignationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeEmployee)
                .WithMany()
                .HasForeignKey(e => e.InchargeEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.PhoneNumber).HasMaxLength(25);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.ToTable("Groups");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EmploymentType).HasMaxLength(50);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Junction table configurations for many-to-many relationships
        modelBuilder.Entity<GroupDepartment>(entity =>
        {
            entity.ToTable("GroupDepartments");
            entity.HasKey(e => new { e.GroupId, e.DepartmentId });
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupDepartments)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupLocation>(entity =>
        {
            entity.ToTable("GroupLocations");
            entity.HasKey(e => new { e.GroupId, e.LocationId });
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupLocations)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupDesignation>(entity =>
        {
            entity.ToTable("GroupDesignations");
            entity.HasKey(e => new { e.GroupId, e.DesignationId });
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupDesignations)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Designation)
                .WithMany()
                .HasForeignKey(e => e.DesignationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupIncharge>(entity =>
        {
            entity.ToTable("GroupIncharges");
            entity.HasKey(e => new { e.GroupId, e.EmployeeId });
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupIncharges)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AssignGroup>(entity =>
        {
            entity.ToTable("AssignGroups");
            entity.Property(e => e.EmployeeType).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Location)
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeCategory)
                .WithMany()
                .HasForeignKey(e => e.InchargeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeDesignation)
                .WithMany()
                .HasForeignKey(e => e.InchargeDesignationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.InchargeEmployee)
                .WithMany()
                .HasForeignKey(e => e.InchargeEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LeaveApplication>(entity =>
        {
            entity.ToTable("LeaveApplications");
            entity.Property(e => e.LeaveType).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(e => e.InchargeApprovalStatus).HasMaxLength(20);
            entity.Property(e => e.LeaveNumber).HasMaxLength(50).IsRequired();
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Incharge)
                .WithMany()
                .HasForeignKey(e => e.InchargeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.CancelledByEmployee)
                .WithMany()
                .HasForeignKey(e => e.CancelledBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MarkDay>(entity =>
        {
            entity.ToTable("MarkDays");
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GroupEmployee>(entity =>
        {
            entity.ToTable("GroupEmployees");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Group)
                .WithMany(g => g.GroupEmployees)
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Employee)
                .WithMany(e => e.GroupEmployees)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances");
            entity.HasIndex(e => new { e.EmployeeId, e.PunchDateTime });
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.ToTable("Modules");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<RoleModule>(entity =>
        {
            entity.ToTable("RoleModules");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RoleModules)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Module)
                .WithMany(m => m.RoleModules)
                .HasForeignKey(e => e.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.RoleId, e.ModuleId }).IsUnique();
        });

        modelBuilder.Entity<RoleAssignment>(entity =>
        {
            entity.ToTable("RoleAssignments");
            entity.Property(e => e.AssignmentType).HasMaxLength(20);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.RoleAssignments)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Group)
                .WithMany()
                .HasForeignKey(e => e.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.RoleId, e.CompanyId, e.AssignmentType, e.GroupId, e.DepartmentId, e.EmployeeId }).IsUnique();
        });

        SeedLookups(modelBuilder);
    }

    private static void SeedLookups(ModelBuilder modelBuilder)
    {
        var countries = new List<Country>
        {
            new() { Id = 1, Name = "Pakistan" },
            new() { Id = 2, Name = "United Arab Emirates" },
            new() { Id = 3, Name = "Saudi Arabia" }
        };

        var cities = new List<City>
        {
            new() { Id = 1, Name = "Karachi", CountryId = 1 },
            new() { Id = 2, Name = "Lahore", CountryId = 1 },
            new() { Id = 3, Name = "Islamabad", CountryId = 1 },
            new() { Id = 4, Name = "Dubai", CountryId = 2 },
            new() { Id = 5, Name = "Abu Dhabi", CountryId = 2 },
            new() { Id = 6, Name = "Sharjah", CountryId = 2 },
            new() { Id = 7, Name = "Riyadh", CountryId = 3 },
            new() { Id = 8, Name = "Jeddah", CountryId = 3 }
        };

        modelBuilder.Entity<Country>().HasData(countries);
        modelBuilder.Entity<City>().HasData(cities);
    }
}
