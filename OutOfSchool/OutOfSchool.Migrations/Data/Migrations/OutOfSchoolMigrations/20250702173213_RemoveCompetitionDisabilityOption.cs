#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class RemoveCompetitionDisabilityOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionOfOptionsForPeopleWithDisabilities",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "OptionsForPeopleWithDisabilities",
                table: "CompetitiveEvents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionOfOptionsForPeopleWithDisabilities",
                table: "CompetitiveEvents",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "OptionsForPeopleWithDisabilities",
                table: "CompetitiveEvents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
