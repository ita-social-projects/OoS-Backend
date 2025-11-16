using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddIndexesToImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Workshops_CoverImageId",
                table: "Workshops",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopImages_ExternalStorageId",
                table: "WorkshopImages",
                column: "ExternalStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopDrafts_CoverImageId",
                table: "WorkshopDrafts",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopDraftImages_ExternalStorageId",
                table: "WorkshopDraftImages",
                column: "ExternalStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventsImages_ExternalStorageId",
                table: "CompetitiveEventsImages",
                column: "ExternalStorageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_CoverImageId",
                table: "CompetitiveEvents",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventDrafts_CoverImageId",
                table: "CompetitiveEventDrafts",
                column: "CoverImageId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventDraftImages_ExternalStorageId",
                table: "CompetitiveEventDraftImages",
                column: "ExternalStorageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workshops_CoverImageId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopImages_ExternalStorageId",
                table: "WorkshopImages");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopDrafts_CoverImageId",
                table: "WorkshopDrafts");

            migrationBuilder.DropIndex(
                name: "IX_WorkshopDraftImages_ExternalStorageId",
                table: "WorkshopDraftImages");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventsImages_ExternalStorageId",
                table: "CompetitiveEventsImages");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEvents_CoverImageId",
                table: "CompetitiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventDrafts_CoverImageId",
                table: "CompetitiveEventDrafts");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventDraftImages_ExternalStorageId",
                table: "CompetitiveEventDraftImages");
        }
    }
}
