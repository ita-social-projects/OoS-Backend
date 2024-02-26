using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddChangeBlockPermissionForAdmins : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "def\n\r !()+,432578>=<?HGIFPQ[]\\rpqons|z{yx}T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n2578(,PQFTno[zxy{}");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "def\n\r !()+,432578>=<?HGIFPQ[]\\rpqon|z{yx}T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n2578(,PQFTn[zxy{}");
    }
}
