using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class ProviderApproval : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<int>(
            name: "Status",
            table: "Providers",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AddColumn<string>(
                name: "License",
                table: "Providers",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<int>(
            name: "LicenseStatus",
            table: "Providers",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "License",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "LicenseStatus",
            table: "Providers");

        migrationBuilder.AlterColumn<bool>(
            name: "Status",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "int",
            oldDefaultValue: 0);
    }
}