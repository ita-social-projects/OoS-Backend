using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class UpdatePermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "e\n26HGIFPQ[\\");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "e\n6HGIFPQ[\\");
    }
}