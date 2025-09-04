#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class MakeExternalRegistryIdOptionalDuringTransition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ExternalRegistryId",
                table: "Individuals",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ExternalRegistryId",
                table: "Individuals",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);
        }
    }
}
