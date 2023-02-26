using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddDefaultValueToIsDeletedColumnInAverageRatingsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AverageRatings",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AverageRatings",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);
    }
}
