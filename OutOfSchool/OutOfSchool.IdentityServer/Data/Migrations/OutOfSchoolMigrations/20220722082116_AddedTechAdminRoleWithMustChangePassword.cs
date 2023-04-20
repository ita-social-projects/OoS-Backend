using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddedTechAdminRoleWithMustChangePassword : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "MustChangePassword",
            table: "AspNetUsers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            columns: new[] { "Description", "RoleName" },
            values: new object[] { "techadmin permissions", "TechAdmin" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "MustChangePassword",
            table: "AspNetUsers");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            columns: new[] { "Description", "RoleName" },
            values: new object[] { "admin permissions", "Admin" });
    }
}