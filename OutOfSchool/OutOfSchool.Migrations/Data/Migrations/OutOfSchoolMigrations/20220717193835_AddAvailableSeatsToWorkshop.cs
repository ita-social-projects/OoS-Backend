using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddAvailableSeatsToWorkshop : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<uint>(
            name: "AvailableSeats",
            table: "Workshops",
            type: "int unsigned",
            nullable: false,
            defaultValue: 4294967295u);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvailableSeats",
            table: "Workshops");
    }
}
