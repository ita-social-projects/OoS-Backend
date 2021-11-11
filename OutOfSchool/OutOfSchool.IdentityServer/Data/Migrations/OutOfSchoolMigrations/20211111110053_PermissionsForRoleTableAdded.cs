using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class PermissionsForRoleTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissionsForRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PackedPermissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsForRoles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 1L, "admin permissions", "de\n\r !()+4325>=<?HGIFPQ[]\\", "Admin" });

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 2L, "provider permissions", "e\n4325HGIFPQ[]\\", "Provider" });

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 3L, "parent permissions", "e\n !()+>=<PQ", "Parent" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionsForRoles");
        }
    }
}
