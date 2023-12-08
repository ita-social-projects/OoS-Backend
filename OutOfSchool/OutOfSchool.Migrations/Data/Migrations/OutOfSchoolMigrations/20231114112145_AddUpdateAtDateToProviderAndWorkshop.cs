using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

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
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "Providers",
            type: "datetime(6)",
            nullable: false)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
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
