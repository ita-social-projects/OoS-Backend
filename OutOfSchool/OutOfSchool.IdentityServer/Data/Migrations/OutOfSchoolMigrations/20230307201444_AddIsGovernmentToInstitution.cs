using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddIsGovernmentToInstitution : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsGovernment",
            table: "Institutions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_QuartzJobs_IsDeleted",
            table: "QuartzJobs",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AverageRatings_IsDeleted",
            table: "AverageRatings",
            column: "IsDeleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_QuartzJobs_IsDeleted",
            table: "QuartzJobs");

        migrationBuilder.DropIndex(
            name: "IX_AverageRatings_IsDeleted",
            table: "AverageRatings");

        migrationBuilder.DropColumn(
            name: "IsGovernment",
            table: "Institutions");
    }
}