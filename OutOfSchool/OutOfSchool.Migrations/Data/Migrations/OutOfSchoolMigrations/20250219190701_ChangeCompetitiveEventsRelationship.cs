#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class ChangeCompetitiveEventsRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEventCoverages_CompetitiveEvents_CompetitiveEvent~",
                table: "CompetitiveEventCoverages");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventCoverages_CompetitiveEventId",
                table: "CompetitiveEventCoverages");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CompetitiveEventId",
                table: "CompetitiveEventCoverages");

            migrationBuilder.AddColumn<int>(
                name: "CoverageId",
                table: "CompetitiveEvents",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_CoverageId",
                table: "CompetitiveEvents",
                column: "CoverageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEventCoverages_CoverageId",
                table: "CompetitiveEvents",
                column: "CoverageId",
                principalTable: "CompetitiveEventCoverages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEventCoverages_CoverageId",
                table: "CompetitiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEvents_CoverageId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CoverageId",
                table: "CompetitiveEvents");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CompetitiveEvents",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitiveEventId",
                table: "CompetitiveEventCoverages",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 1,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 2,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 5,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventCoverages",
                keyColumn: "Id",
                keyValue: 6,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventCoverages_CompetitiveEventId",
                table: "CompetitiveEventCoverages",
                column: "CompetitiveEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEventCoverages_CompetitiveEvents_CompetitiveEvent~",
                table: "CompetitiveEventCoverages",
                column: "CompetitiveEventId",
                principalTable: "CompetitiveEvents",
                principalColumn: "Id");
        }
    }
}
