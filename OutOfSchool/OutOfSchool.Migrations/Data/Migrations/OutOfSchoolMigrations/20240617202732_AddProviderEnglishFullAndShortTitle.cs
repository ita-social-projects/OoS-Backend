using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class AddProviderEnglishFullAndShortTitle : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
                name: "ProviderTitle",
                table: "Workshops",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "ProviderTitleEn",
                table: "Workshops",
                type: "varchar(60)",
                maxLength: 60,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "FullTitleEn",
                table: "Providers",
                type: "varchar(60)",
                maxLength: 60,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "ShortTitleEn",
                table: "Providers",
                type: "varchar(60)",
                maxLength: 60,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProviderTitleEn",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "FullTitleEn",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ShortTitleEn",
            table: "Providers");

        migrationBuilder.AlterColumn<string>(
                name: "ProviderTitle",
                table: "Workshops",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(60)",
                oldMaxLength: 60)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }
}