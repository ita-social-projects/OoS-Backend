using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddTitleEnColumnToAchievementTypeModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "TitleEn",
            table: "AchievementTypes",
            type: "varchar(200)",
            maxLength: 200,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 1L,
            column: "TitleEn",
            value: "Winners of international and all-Ukrainian sports competitions (individual and team)");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 2L,
            column: "TitleEn",
            value: "Winners and participants of international, all-Ukrainian and regional contests and exhibitions of scientific, technical, research, innovation, IT projects");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 3L,
            column: "TitleEn",
            value: "Recipients of international grants");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 4L,
            column: "TitleEn",
            value: "Winners of international cultural competitions and festivals");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 5L,
            column: "TitleEn",
            value: "Socially active categories of students");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 6L,
            column: "TitleEn",
            value: "Google digital tools for institutions of higher and professional pre-higher education");

        migrationBuilder.UpdateData(
            table: "AchievementTypes",
            keyColumn: "Id",
            keyValue: 7L,
            column: "TitleEn",
            value: "Winners and participants of olympiads at the international and all-Ukrainian levels");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "TitleEn",
            table: "AchievementTypes");
    }
}
