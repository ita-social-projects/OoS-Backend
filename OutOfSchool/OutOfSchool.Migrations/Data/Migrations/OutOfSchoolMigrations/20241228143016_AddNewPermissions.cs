using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddNewPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5mgoaOi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5k=");
        }
    }
}
