using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddSoftDeleteToAdmins : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionAdmins",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "AreaAdmins",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionAdmins_IsDeleted",
            table: "InstitutionAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AreaAdmins_IsDeleted",
            table: "AreaAdmins",
            column: "IsDeleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_InstitutionAdmins_IsDeleted",
            table: "InstitutionAdmins");

        migrationBuilder.DropIndex(
            name: "IX_AreaAdmins_IsDeleted",
            table: "AreaAdmins");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "InstitutionAdmins");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "AreaAdmins");
    }
}
