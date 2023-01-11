using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddEnColumnToSocialGroupAndAchievementTypeModels : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Name",
            keyValue: null,
            column: "Name",
            value: "");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(100)",
            oldMaxLength: 100,
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
            name: "NameEn",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

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

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 1L,
            column: "NameEn",
            value: "Children from large families");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 2L,
            column: "NameEn",
            value: "Children from low-income families");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 3L,
            column: "NameEn",
            value: "Children with disabilities");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 4L,
            column: "NameEn",
            value: "Orphans");

        migrationBuilder.UpdateData(
            table: "SocialGroups",
            keyColumn: "Id",
            keyValue: 5L,
            column: "NameEn",
            value: "Children deprived of parental care");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NameEn",
            table: "SocialGroups");

        migrationBuilder.DropColumn(
            name: "TitleEn",
            table: "AchievementTypes");

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "SocialGroups",
            type: "varchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(100)",
            oldMaxLength: 100)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }
}
