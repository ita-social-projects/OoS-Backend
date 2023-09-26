using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddPropertyNameOldValueNewValueInProvideradmincnageslog : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PropertyName",
            table: "ProviderAdminChangesLog",
            type: "varchar(128)",
            maxLength: 128,
            nullable: false)
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.AddColumn<string>(
            name: "OldValue",
            table: "ProviderAdminChangesLog",
            type: "varchar(500)",
            maxLength: 500,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
        migrationBuilder.AddColumn<string>(
            name: "NewValue",
            table: "ProviderAdminChangesLog",
            type: "varchar(500)",
            maxLength: 500,
            nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }
}
