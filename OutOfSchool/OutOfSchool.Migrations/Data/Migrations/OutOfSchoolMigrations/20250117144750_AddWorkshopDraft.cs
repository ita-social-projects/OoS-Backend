using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddWorkshopDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CompetitiveEvents",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkshopDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    CoverImageId = table.Column<string>(type: "char(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DraftStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    RejectionMessage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopDraftContent = table.Column<string>(type: "json", nullable: false)
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
                    table.PrimaryKey("PK_WorkshopDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopDrafts_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopDrafts_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TeacherDraft",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageId = table.Column<string>(type: "char(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopDraftId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    IsDefaultTeacher = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDraft", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherDraft_WorkshopDrafts_WorkshopDraftId",
                        column: x => x.WorkshopDraftId,
                        principalTable: "WorkshopDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkshopDraftImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopDraftImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_WorkshopDraftImages_WorkshopDrafts_EntityId",
                        column: x => x.EntityId,
                        principalTable: "WorkshopDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDraft_WorkshopDraftId",
                table: "TeacherDraft",
                column: "WorkshopDraftId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopDrafts_ProviderId",
                table: "WorkshopDrafts",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopDrafts_WorkshopId",
                table: "WorkshopDrafts",
                column: "WorkshopId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherDraft");

            migrationBuilder.DropTable(
                name: "WorkshopDraftImages");

            migrationBuilder.DropTable(
                name: "WorkshopDrafts");

            migrationBuilder.UpdateData(
                table: "CompetitiveEvents",
                keyColumn: "Title",
                keyValue: null,
                column: "Title",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "CompetitiveEvents",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
