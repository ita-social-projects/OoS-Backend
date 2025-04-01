#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class ChangeWorkshopDraftFieldLength : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
                name: "RejectionMessage",
                table: "WorkshopDrafts",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "WorkshopDrafts",
                type: "char(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(36)",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "TeacherDraft",
                type: "char(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(36)",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<decimal>(
            name: "Rate",
            table: "AverageRatings",
            type: "decimal(2,1)",
            precision: 2,
            scale: 1,
            nullable: false,
            oldClrType: typeof(float),
            oldType: "float");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
                name: "RejectionMessage",
                table: "WorkshopDrafts",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(500)",
                oldMaxLength: 500,
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "WorkshopDrafts",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(255)",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "TeacherDraft",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "char(255)",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<float>(
            name: "Rate",
            table: "AverageRatings",
            type: "float",
            nullable: false,
            oldClrType: typeof(decimal),
            oldType: "decimal(2,1)",
            oldPrecision: 2,
            oldScale: 1);
    }
}