using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddLanguageSelector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "LanguageOfEducationId",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_LanguageOfEducationId",
                table: "Workshops",
                column: "LanguageOfEducationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops",
                column: "LanguageOfEducationId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Languages_LanguageOfEducationId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_LanguageOfEducationId",
                table: "Workshops");

            migrationBuilder.AlterColumn<uint>(
                name: "LanguageOfEducationId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
