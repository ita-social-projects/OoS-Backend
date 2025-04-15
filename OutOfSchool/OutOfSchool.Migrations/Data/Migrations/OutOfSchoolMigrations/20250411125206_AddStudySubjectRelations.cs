using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddStudySubjectRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProviderId",
                table: "StudySubjects",
                type: "UUID(16)",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "StudySubjectWorkshop",
                columns: table => new
                {
                    StudySubjectsId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    WorkshopsId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySubjectWorkshop", x => new { x.StudySubjectsId, x.WorkshopsId });
                    table.ForeignKey(
                        name: "FK_StudySubjectWorkshop_StudySubjects_StudySubjectsId",
                        column: x => x.StudySubjectsId,
                        principalTable: "StudySubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudySubjectWorkshop_Workshops_WorkshopsId",
                        column: x => x.WorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjects_ProviderId",
                table: "StudySubjects",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_StudySubjectWorkshop_WorkshopsId",
                table: "StudySubjectWorkshop",
                column: "WorkshopsId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySubjects_Providers_ProviderId",
                table: "StudySubjects",
                column: "ProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudySubjects_Providers_ProviderId",
                table: "StudySubjects");

            migrationBuilder.DropTable(
                name: "StudySubjectWorkshop");

            migrationBuilder.DropIndex(
                name: "IX_StudySubjects_ProviderId",
                table: "StudySubjects");

            migrationBuilder.DropColumn(
                name: "ProviderId",
                table: "StudySubjects");
        }
    }
}
