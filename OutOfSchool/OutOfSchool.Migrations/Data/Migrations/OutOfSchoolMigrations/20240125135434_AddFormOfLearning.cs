using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

public partial class AddFormOfLearning : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FormOfLearning",
            table: "Workshops",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.Sql("UPDATE workshops SET formoflearning = 10;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FormOfLearning",
            table: "Workshops");
    }
}
