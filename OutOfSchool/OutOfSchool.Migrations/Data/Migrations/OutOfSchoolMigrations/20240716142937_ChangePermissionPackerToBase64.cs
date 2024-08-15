using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class ChangePermissionPackerToBase64 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11ccnBxb258ent5eH2Qjo+NjJFUZw==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 2L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVA==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 3L,
            column: "PackedPermissions",
            value: "ZQMKCwwUFhUXHiAfISgpKz49PFBRVA==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzI2SEdJRlBRW1xU");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoLBRQUUZUblt6eHl7fY6MjY+RZw==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW2c=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlSMjVtn");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "def\n\r !()+,432578>=<?HGIFPQ[]\\rpqon|z{yx}Tg");

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
            value: "ef\n2578(,PQFTn[zxy{}g");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFTxy[g");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ef\n2578(PQFT[g");
    }
}