using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddPayRate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsPerMonth",
            table: "Workshops");

        migrationBuilder.AddColumn<int>(
            name: "PayRate",
            table: "Workshops",
            type: "int",
            nullable: false,
            defaultValue: 1);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "PayRate",
            table: "Workshops");

        migrationBuilder.AddColumn<bool>(
            name: "IsPerMonth",
            table: "Workshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);
    }
}