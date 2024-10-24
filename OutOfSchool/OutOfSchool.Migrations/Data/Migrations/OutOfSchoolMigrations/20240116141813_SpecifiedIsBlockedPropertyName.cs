using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class SpecifiedIsBlockedPropertyName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "IsBlocked",
            table: "ChatRoomWorkshops",
            newName: "IsBlockedByProvider");

        migrationBuilder.RenameColumn(
            name: "IsBlocked",
            table: "Applications",
            newName: "IsBlockedByProvider");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "IsBlockedByProvider",
            table: "ChatRoomWorkshops",
            newName: "IsBlocked");

        migrationBuilder.RenameColumn(
            name: "IsBlockedByProvider",
            table: "Applications",
            newName: "IsBlocked");
    }
}
