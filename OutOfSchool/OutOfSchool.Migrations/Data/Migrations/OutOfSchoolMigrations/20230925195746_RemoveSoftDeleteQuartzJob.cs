using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class RemoveSoftDeleteQuartzJob : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_QuartzJobs_IsDeleted",
            table: "QuartzJobs");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "QuartzJobs");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "QuartzJobs",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_QuartzJobs_IsDeleted",
            table: "QuartzJobs",
            column: "IsDeleted");
    }
}
