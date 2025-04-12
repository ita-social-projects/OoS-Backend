#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AuthorizationAndProviderRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_AspNetUsers_UserId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "EmployeeWorkshop");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Providers_UserId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Providers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Providers",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    BlockingType = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Employees_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "EmployeeWorkshop",
                columns: table => new
                {
                    EmployeesUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagedWorkshopsId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkshop", x => new { x.EmployeesUserId, x.ManagedWorkshopsId });
                    table.ForeignKey(
                        name: "FK_EmployeeWorkshop_Employees_EmployeesUserId",
                        column: x => x.EmployeesUserId,
                        principalTable: "Employees",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeWorkshop_Workshops_ManagedWorkshopsId",
                        column: x => x.ManagedWorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_UserId",
                table: "Providers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsDeleted",
                table: "Employees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ProviderId",
                table: "Employees",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeWorkshop_ManagedWorkshopsId",
                table: "EmployeeWorkshop",
                column: "ManagedWorkshopsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_AspNetUsers_UserId",
                table: "Providers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
