using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedModulesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete existing modules first to avoid PK conflicts
            migrationBuilder.Sql("DELETE FROM [Modules] WHERE [Id] IN (1,2,3,4,5,6,7,8,9)");

            var now = DateTime.UtcNow;

            migrationBuilder.InsertData(
                table: "Modules",
                columns: new[] { "Id", "Name", "DisplayName", "Description", "Area", "Controller", "Action", "Icon", "SortOrder", "IsActive", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { 1, "Dashboard", "Dashboard", "Main dashboard overview", "Administrator", "Dashboard", "Index", "bi-speedometer2", 1, true, now, null },
                    { 2, "Administrator", "Administrator", "Administrator module for company management", "Administrator", "Dashboard", "Lookups", "bi-building-gear", 2, true, now, null },
                    { 3, "SuperUser", "Super User", "Super User administration module", "SuperUser", "Dashboard", "Index", "bi-shield-lock", 3, true, now, null },
                    { 4, "Leave", "Leave", "Leave management module", "Administrator", "Leave", "Index", "bi-calendar3", 4, true, now, null },
                    { 5, "GenIncharge", "Gen Incharge", "General Incharge module", "Administrator", "GenIncharge", "Index", "bi-person-workspace", 5, true, now, null },
                    { 6, "GenEmployee", "Gen Employee", "General Employee module", "Administrator", "GenEmployee", "Index", "bi-briefcase", 6, true, now, null },
                    { 7, "List", "List", "List/Reports module", "Administrator", "List", "Index", "bi-list-check", 7, true, now, null },
                    { 8, "Report", "Report", "Reporting module", "Administrator", "Report", "Index", "bi-graph-up", 8, true, now, null },
                    { 9, "Process", "Process", "Process management module", "Administrator", "Process", "Index", "bi-arrow-repeat", 9, true, now, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Modules",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        }
    }
}
