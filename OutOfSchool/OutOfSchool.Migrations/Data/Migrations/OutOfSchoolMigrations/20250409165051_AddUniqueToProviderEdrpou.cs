#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddUniqueToProviderEdrpou : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers",
                column: "Edrpou",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers",
                column: "Edrpou");
        }
    }
}
