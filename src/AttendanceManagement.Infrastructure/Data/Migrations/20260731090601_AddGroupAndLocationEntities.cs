using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupAndLocationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EmploymentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    JoiningDateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JoiningDateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEndDateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEndDateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfBirthFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfBirthTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FilterByDepartment = table.Column<bool>(type: "bit", nullable: false),
                    FilterByDesignation = table.Column<bool>(type: "bit", nullable: false),
                    FilterByLocation = table.Column<bool>(type: "bit", nullable: false),
                    FilterByIncharge = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Groups_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupDepartments",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDepartments", x => new { x.GroupId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_GroupDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupDepartments_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupDesignations",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    DesignationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupDesignations", x => new { x.GroupId, x.DesignationId });
                    table.ForeignKey(
                        name: "FK_GroupDesignations_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupDesignations_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupIncharges",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupIncharges", x => new { x.GroupId, x.EmployeeId });
                    table.ForeignKey(
                        name: "FK_GroupIncharges_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupIncharges_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupLocations",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLocations", x => new { x.GroupId, x.LocationId });
                    table.ForeignKey(
                        name: "FK_GroupLocations_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupLocations_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupDepartments_DepartmentId",
                table: "GroupDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupDesignations_DesignationId",
                table: "GroupDesignations",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupIncharges_EmployeeId",
                table: "GroupIncharges",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLocations_LocationId",
                table: "GroupLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CompanyId",
                table: "Groups",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupDepartments");

            migrationBuilder.DropTable(
                name: "GroupDesignations");

            migrationBuilder.DropTable(
                name: "GroupIncharges");

            migrationBuilder.DropTable(
                name: "GroupLocations");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
