using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddStudyPeriodFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "StudyPeriodStartDate",
                table: "Workshops",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 9, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StudyPeriodEndDate",
                table: "Workshops",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 5, 31));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudyPeriodStartDate",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "StudyPeriodEndDate",
                table: "Workshops");
        }
    }
}
