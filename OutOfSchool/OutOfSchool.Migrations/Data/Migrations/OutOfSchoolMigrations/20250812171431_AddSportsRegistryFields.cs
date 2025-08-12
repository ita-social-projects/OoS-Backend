#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddSportsRegistryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "SportRegistryIdCode",
                table: "InstitutionHierarchies",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SportsSectionNumeral",
                table: "InstitutionHierarchies",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SportRegistryIdCode",
                table: "InstitutionHierarchies");

            migrationBuilder.DropColumn(
                name: "SportsSectionNumeral",
                table: "InstitutionHierarchies");
        }
    }
}
