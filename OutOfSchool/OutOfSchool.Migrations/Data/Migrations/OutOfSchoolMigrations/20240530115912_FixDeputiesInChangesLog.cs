using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class FixDeputiesInChangesLog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ProviderAdminChangesLog_Workshops_ManagedWorkshopId",
            table: "ProviderAdminChangesLog");

        migrationBuilder.DropIndex(
            name: "IX_ProviderAdminChangesLog_ManagedWorkshopId",
            table: "ProviderAdminChangesLog");

        migrationBuilder.DropColumn(
            name: "ManagedWorkshopId",
            table: "ProviderAdminChangesLog");

        migrationBuilder.AddColumn<bool>(
            name: "IsDeputy",
            table: "ProviderAdminChangesLog",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsDeputy",
            table: "ProviderAdminChangesLog");

        migrationBuilder.AddColumn<Guid>(
            name: "ManagedWorkshopId",
            table: "ProviderAdminChangesLog",
            type: "binary(16)",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_ProviderAdminChangesLog_ManagedWorkshopId",
            table: "ProviderAdminChangesLog",
            column: "ManagedWorkshopId");

        migrationBuilder.AddForeignKey(
            name: "FK_ProviderAdminChangesLog_Workshops_ManagedWorkshopId",
            table: "ProviderAdminChangesLog",
            column: "ManagedWorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id");
    }
}
