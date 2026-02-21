#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class SeedNewCompEventPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg5Pj08P0hHSUZQUVtdXF5ycHFvbnx6e3l4fZCOj42MkVRnlpo=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 4L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzI2SEdJRlBRW1xUlpiXmQ==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoLDkUUFFGVG5bXnp4eXt9joyNj5Fnlpo=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1Nzg5KBRQUUZUeHmOjI2PkVteZ5aa");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1Nzg5KBRQUUZUjI1bXmeWmg==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 8L,
                column: "PackedPermissions",
                value: "ZTI3OVpbXmZUlpo=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg5Pj08P0hHSUZQUVtdXF5ycHFvbnx6e3l4fZCOj42MkVRnlg==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 4L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzI2SEdJRlBRW1xUlg==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoLDkUUFFGVG5bXnp4eXt9joyNj5Fnlg==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1Nzg5KBRQUUZUeHmOjI2PkVteZ5Y=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1Nzg5KBRQUUZUjI1bXmeW");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 8L,
                column: "PackedPermissions",
                value: "ZTI3OVpbXmZU");
        }
    }
}
