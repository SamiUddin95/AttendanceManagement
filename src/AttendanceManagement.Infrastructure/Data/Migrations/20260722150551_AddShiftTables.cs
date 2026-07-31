using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShiftTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShiftId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsWorking = table.Column<bool>(type: "bit", nullable: false),
                    IsAlternate = table.Column<bool>(type: "bit", nullable: false),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false),
                    IsOther = table.Column<bool>(type: "bit", nullable: false),
                    OtherHours = table.Column<int>(type: "int", nullable: true),
                    OtherMinutes = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: true),
                    LunchBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    PrayerBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    TeaBreak = table.Column<TimeSpan>(type: "time", nullable: true),
                    FlexiIn = table.Column<TimeSpan>(type: "time", nullable: true),
                    FlexiOut = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsFlexible = table.Column<bool>(type: "bit", nullable: false),
                    CopyFromPrevious = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShiftDays_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShiftDays_ShiftId",
                table: "ShiftDays",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_CompanyId",
                table: "Shifts",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShiftDays");

            migrationBuilder.DropTable(
                name: "Shifts");
        }
    }
}
