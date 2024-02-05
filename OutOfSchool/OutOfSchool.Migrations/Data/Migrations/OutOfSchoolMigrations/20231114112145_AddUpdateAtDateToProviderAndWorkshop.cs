using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddUpdateAtDateToProviderAndWorkshop : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Workshops",
            type: "datetime(6)",
            nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Providers",
            type: "datetime(6)",
            nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "UpdatedAt",
            table: "Providers");
    }
}
