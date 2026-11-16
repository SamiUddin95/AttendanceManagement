using AttendanceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations;

[DbContext(typeof(AttendanceDbContext))]
[Migration("20261116122000_MakeMarkDayGroupOptional")]
public partial class MakeMarkDayGroupOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<int>(name: "GroupId", table: "MarkDays", type: "int", nullable: true, oldClrType: typeof(int), oldType: "int");

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.AlterColumn<int>(name: "GroupId", table: "MarkDays", type: "int", nullable: false, defaultValue: 0, oldClrType: typeof(int), oldType: "int", oldNullable: true);
}
