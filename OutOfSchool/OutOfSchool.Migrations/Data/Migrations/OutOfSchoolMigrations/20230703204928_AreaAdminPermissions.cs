using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AreaAdminPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 7L, "area admin permissions", "ef\n257(PQFT[", "AreaAdmin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 7L);
        }
    }
}
