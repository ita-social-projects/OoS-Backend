using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddBlockPhoneNumberToProvider : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "BlockPhoneNumber",
            table: "Providers",
            type: "varchar(15)",
            maxLength: 15,
            nullable: false,
            defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "BlockPhoneNumber",
            table: "Providers");
    }
}
