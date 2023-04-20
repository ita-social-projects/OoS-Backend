using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class UpdatePersonalInfoPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "de\n\r !()+43257>=<?HGIFPQ[]\\rpqoT");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 2L,
            column: "PackedPermissions",
            value: "e\n43256HGIFPQ[]\\T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 3L,
            column: "PackedPermissions",
            value: "e\n !()+>=<PQT");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "e\n26HGIFPQ[\\T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "e\n257(PQFT");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "de\n\r !()+43258>=<?HGIFPQ[]\\rpqof");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 2L,
            column: "PackedPermissions",
            value: "e\n43256HGIFPQ[]\\7");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 3L,
            column: "PackedPermissions",
            value: "e\n !()+>=<PQ,");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "e\n26HGIFPQ[\\7");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "e\n258(PQFs");
    }
}