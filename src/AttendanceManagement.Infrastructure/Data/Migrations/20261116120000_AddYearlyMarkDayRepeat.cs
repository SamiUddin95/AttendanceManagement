using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations;

[DbContext(typeof(AttendanceDbContext))]
[Migration("20261116120000_AddYearlyMarkDayRepeat")]
public partial class AddYearlyMarkDayRepeat : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AddColumn<bool>(name: "RepeatsYearly", table: "MarkDays", type: "bit", nullable: false, defaultValue: false);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.DropColumn(name: "RepeatsYearly", table: "MarkDays");
}
