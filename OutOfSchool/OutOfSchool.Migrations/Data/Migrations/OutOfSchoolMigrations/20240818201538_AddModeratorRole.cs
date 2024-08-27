using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddModeratorRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11cXnJwcW9ufHp7eXh9kI6PjYyRVGc=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoLBRQUUZUblteenh5e32OjI2PkWc=");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW15n");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 7L,
                column: "PackedPermissions",
                value: "ZWYDAgEECjI1NzgoFFBRRlSMjVteZw==");

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 8L, "moderator permissions", "MjdaXlQ=", "Moderator" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 8L);

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11ccnBxb258ent5eH2Qjo+NjJFUZw==");

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
    }
}
