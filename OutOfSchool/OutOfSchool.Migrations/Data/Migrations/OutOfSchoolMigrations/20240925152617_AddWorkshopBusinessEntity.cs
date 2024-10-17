using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class AddWorkshopBusinessEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Workshops",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)")
            .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AddColumn<DateOnly>(
            name: "ActiveFrom",
            table: "Workshops",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1));

        migrationBuilder.AddColumn<DateOnly>(
            name: "ActiveTo",
            table: "Workshops",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(9999, 12, 31));

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Workshops",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Workshops",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "DeleteDate",
            table: "Workshops",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Workshops",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Document",
                table: "Workshops",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "File",
                table: "Workshops",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<bool>(
            name: "IsSystemProtected",
            table: "Workshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Workshops",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ActiveFrom",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "ActiveTo",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "DeleteDate",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "Document",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "File",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "IsSystemProtected",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "ModifiedBy",
            table: "Workshops");

        migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Workshops",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
    }
}