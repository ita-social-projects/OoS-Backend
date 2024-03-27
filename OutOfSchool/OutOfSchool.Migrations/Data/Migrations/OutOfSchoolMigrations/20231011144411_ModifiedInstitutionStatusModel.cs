using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class ModifiedInstitutionStatusModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "InstitutionStatuses",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.UpdateData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 1L,
            column: "NameEn",
            value: "Active");

        migrationBuilder.UpdateData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 2L,
            column: "NameEn",
            value: "Undergoing reorganization");

        migrationBuilder.UpdateData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 3L,
            column: "NameEn",
            value: "Waiting for reorganization");

        migrationBuilder.UpdateData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 4L,
            columns: new[] { "Name", "NameEn" },
            values: new object[] { "Відсутній статус", "Without status" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "NameEn",
            table: "InstitutionStatuses");

        migrationBuilder.UpdateData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 4L,
            column: "Name",
            value: "Відсутній");
    }
}