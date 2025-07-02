using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddProviderDataReadPermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                value: "Mjc5WlteZlQ=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11cXnJwcW9ufHp7eXh9kI6PjYyRVGeW");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoLBRQUUZUblteenh5e32OjI2PkWeW");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW15nlg==");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoFFBRRlSMjVteZ5Y=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 8L,
                column: "PackedPermissions",
                value: "MjdaW15mVA==");
        }
    }
}
