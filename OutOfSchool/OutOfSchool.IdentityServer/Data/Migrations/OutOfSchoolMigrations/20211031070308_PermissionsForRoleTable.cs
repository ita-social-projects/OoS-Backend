using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class PermissionsForRoleTable : Migration
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
                values: new object[] { 1L, "admin permissions", "d\n\r !()+4325>=<?HGIPQ[]\\", "admin" });

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 2L, "provider permissions", "\n4325HGIPQ[]\\", "provider" });

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 3L, "parent permissions", "\r !()+>=<PQ", "parent" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissionsForRoles");
        }
    }
}
