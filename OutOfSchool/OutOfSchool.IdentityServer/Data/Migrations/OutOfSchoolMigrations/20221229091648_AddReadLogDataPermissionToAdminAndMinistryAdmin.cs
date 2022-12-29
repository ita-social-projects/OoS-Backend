using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AddReadLogDataPermissionToAdminAndMinistryAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "def\n\r !()+43257>=<?HGIFPQ[]\\rpqonT");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "ef\n257(PQFTn[");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "de\n\r !()+43257>=<?HGIFPQ[]\\rpqonT");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "e\n257(PQFTn[");
        }
    }
}
