using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddNameEnColumnToSocialGroupModel : Migration
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
