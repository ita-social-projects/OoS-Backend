using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class RemoveSoftDeleteFromCompanyInformation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            DELETE FROM CompanyInformationItems WHERE IsDeleted = 1;
            DELETE FROM CompanyInformation WHERE IsDeleted = 1;");

        migrationBuilder.DropIndex(
            name: "IX_CompanyInformationItems_IsDeleted",
            table: "CompanyInformationItems");

        migrationBuilder.DropIndex(
            name: "IX_CompanyInformation_IsDeleted",
            table: "CompanyInformation");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "CompanyInformationItems");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "CompanyInformation");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformationItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformation",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformationItems_IsDeleted",
            table: "CompanyInformationItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformation_IsDeleted",
            table: "CompanyInformation",
            column: "IsDeleted");
    }
}
