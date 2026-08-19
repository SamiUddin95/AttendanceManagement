using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignGroupEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    EmployeeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    InchargeCategoryId = table.Column<int>(type: "int", nullable: true),
                    InchargeDesignationId = table.Column<int>(type: "int", nullable: true),
                    InchargeEmployeeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Categories_InchargeCategoryId",
                        column: x => x.InchargeCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Designations_InchargeDesignationId",
                        column: x => x.InchargeDesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Employees_InchargeEmployeeId",
                        column: x => x.InchargeEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssignGroups_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_CompanyId",
                table: "AssignGroups",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_DepartmentId",
                table: "AssignGroups",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_GroupId",
                table: "AssignGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_InchargeCategoryId",
                table: "AssignGroups",
                column: "InchargeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_InchargeDesignationId",
                table: "AssignGroups",
                column: "InchargeDesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_InchargeEmployeeId",
                table: "AssignGroups",
                column: "InchargeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignGroups_LocationId",
                table: "AssignGroups",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignGroups");
        }
    }
}
