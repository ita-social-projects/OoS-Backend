#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class CompetitiveEventsModerationFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompetitiveEventDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    CompetitiveEventId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    DraftStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverageId = table.Column<int>(type: "int", nullable: false),
                    CompetitiveEventAccountingTypeId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CoverImageId = table.Column<string>(type: "char(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompetitiveEventDraftContent = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEventDrafts_CompetitiveEvents_CompetitiveEventId",
                        column: x => x.CompetitiveEventId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompetitiveEventDrafts_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitiveEventDraftImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventDraftImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_CompetitiveEventDraftImages_CompetitiveEventDrafts_EntityId",
                        column: x => x.EntityId,
                        principalTable: "CompetitiveEventDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventDrafts_CompetitiveEventId",
                table: "CompetitiveEventDrafts",
                column: "CompetitiveEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventDrafts_ProviderId",
                table: "CompetitiveEventDrafts",
                column: "ProviderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitiveEventDraftImages");

            migrationBuilder.DropTable(
                name: "CompetitiveEventDrafts");
        }
    }
}
