#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class RemoveCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops",
                column: "LanguageOfEducationId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops",
                column: "LanguageOfEducationId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
