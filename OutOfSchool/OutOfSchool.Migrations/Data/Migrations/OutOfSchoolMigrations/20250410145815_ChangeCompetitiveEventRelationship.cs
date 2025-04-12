#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class ChangeCompetitiveEventRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Providers_CompetitiveEvents_CompetitiveEventId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_CompetitiveEventId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "CompetitiveEventId",
                table: "Providers");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                type: "UUID(16)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "UUID(16)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                column: "OrganizerOfTheEventId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitiveEventId",
                table: "Providers",
                type: "UUID(16)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                type: "UUID(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "UUID(16)");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_CompetitiveEventId",
                table: "Providers",
                column: "CompetitiveEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                column: "OrganizerOfTheEventId",
                principalTable: "Providers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_CompetitiveEvents_CompetitiveEventId",
                table: "Providers",
                column: "CompetitiveEventId",
                principalTable: "CompetitiveEvents",
                principalColumn: "Id");
        }
    }
}
