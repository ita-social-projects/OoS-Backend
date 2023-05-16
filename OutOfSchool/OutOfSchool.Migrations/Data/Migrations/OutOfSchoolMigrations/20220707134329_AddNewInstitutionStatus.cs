using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddNewInstitutionStatus : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "InstitutionStatuses",
            columns: new[] { "Id", "Name" },
            values: new object[] { 4L, "Відсутній" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "InstitutionStatuses",
            keyColumn: "Id",
            keyValue: 4L);
    }
}
