using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Leaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    LeavePolicyId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaves_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leaves_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leaves_LeavePolicies_LeavePolicyId",
                        column: x => x.LeavePolicyId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NoOfDays = table.Column<int>(type: "int", nullable: false),
                    CarryForward = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveDetails_Leaves_LeaveId",
                        column: x => x.LeaveId,
                        principalTable: "Leaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveDetails_LeaveId",
                table: "LeaveDetails",
                column: "LeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_CategoryId",
                table: "Leaves",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_CompanyId",
                table: "Leaves",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_LeavePolicyId",
                table: "Leaves",
                column: "LeavePolicyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveDetails");

            migrationBuilder.DropTable(
                name: "Leaves");
        }
    }
}
