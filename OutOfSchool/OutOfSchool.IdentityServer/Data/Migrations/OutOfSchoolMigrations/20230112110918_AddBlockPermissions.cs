using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AddBlockPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "de\n\r !()+43257>=<?HGIFPQ[]\\rpqo|z{yx}T");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "e\n257(PQFTzxy{}");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ef\n257(PQFTy[");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "de\n\r !()+43257>=<?HGIFPQ[]\\rpqo|z{yxT");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "PackedPermissions",
                value: "e\n257(PQFT");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "PackedPermissions",
                value: "ef\n257(PQFTxy[");
        }
    }
}
