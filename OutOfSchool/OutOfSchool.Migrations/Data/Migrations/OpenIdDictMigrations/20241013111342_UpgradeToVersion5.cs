using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OpenIdDictMigrations;

/// <inheritdoc />
public partial class UpgradeToVersion5 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Type",
            table: "OpenIddictApplications",
            newName: "ClientType");

        migrationBuilder.AddColumn<string>(
                name: "ApplicationType",
                table: "OpenIddictApplications",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "JsonWebKeySet",
                table: "OpenIddictApplications",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "OpenIddictApplications",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ApplicationType",
            table: "OpenIddictApplications");

        migrationBuilder.DropColumn(
            name: "JsonWebKeySet",
            table: "OpenIddictApplications");

        migrationBuilder.DropColumn(
            name: "Settings",
            table: "OpenIddictApplications");

        migrationBuilder.RenameColumn(
            name: "ClientType",
            table: "OpenIddictApplications",
            newName: "Type");
    }
}