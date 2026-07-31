using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Permanent"),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    DesignationId = table.Column<int>(type: "int", nullable: true),
                    ShiftId = table.Column<int>(type: "int", nullable: true),
                    LeaveId = table.Column<int>(type: "int", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PeriodEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResignationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EfficiencyRequired = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    InchargeCategoryId = table.Column<int>(type: "int", nullable: true),
                    InchargeDesignationId = table.Column<int>(type: "int", nullable: true),
                    InchargeEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ImageBase64 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CardNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FatherSpouseName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BloodGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Cell = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OverTimeEntitled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Categories_InchargeCategoryId",
                        column: x => x.InchargeCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Designations_DesignationId",
                        column: x => x.DesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Designations_InchargeDesignationId",
                        column: x => x.InchargeDesignationId,
                        principalTable: "Designations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Employees_InchargeEmployeeId",
                        column: x => x.InchargeEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Leaves_LeaveId",
                        column: x => x.LeaveId,
                        principalTable: "Leaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Employees_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CategoryId",
                table: "Employees",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyId",
                table: "Employees",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DesignationId",
                table: "Employees",
                column: "DesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_InchargeCategoryId",
                table: "Employees",
                column: "InchargeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_InchargeDesignationId",
                table: "Employees",
                column: "InchargeDesignationId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_InchargeEmployeeId",
                table: "Employees",
                column: "InchargeEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_LeaveId",
                table: "Employees",
                column: "LeaveId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShiftId",
                table: "Employees",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Employees");
        }
    }
}
