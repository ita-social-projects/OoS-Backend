using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class RemoveLanguagesFromStudySubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySubjects_Languages_PrimaryLanguageId",
                table: "StudySubjects");

            migrationBuilder.DropTable(
                name: "LanguageStudySubject");

            migrationBuilder.RenameColumn(
                name: "PrimaryLanguageId",
                table: "StudySubjects",
                newName: "LanguageId");

            migrationBuilder.RenameColumn(
                name: "IsPrimaryLanguageUkrainian",
                table: "StudySubjects",
                newName: "IsLanguageUkrainian");

            migrationBuilder.RenameIndex(
                name: "IX_StudySubjects_PrimaryLanguageId",
                table: "StudySubjects",
                newName: "IX_StudySubjects_LanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId",
                principalTable: "InstitutionHierarchies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySubjects_Languages_LanguageId",
                table: "StudySubjects",
                column: "LanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySubjects_Languages_LanguageId",
                table: "StudySubjects");

            migrationBuilder.RenameColumn(
                name: "LanguageId",
                table: "StudySubjects",
                newName: "PrimaryLanguageId");

            migrationBuilder.RenameColumn(
                name: "IsLanguageUkrainian",
                table: "StudySubjects",
                newName: "IsPrimaryLanguageUkrainian");

            migrationBuilder.RenameIndex(
                name: "IX_StudySubjects_LanguageId",
                table: "StudySubjects",
                newName: "IX_StudySubjects_PrimaryLanguageId");

            migrationBuilder.CreateTable(
                name: "LanguageStudySubject",
                columns: table => new
                {
                    LanguagesId = table.Column<long>(type: "bigint", nullable: false),
                    StudySubjectId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageStudySubject", x => new { x.LanguagesId, x.StudySubjectId });
                    table.ForeignKey(
                        name: "FK_LanguageStudySubject_Languages_LanguagesId",
                        column: x => x.LanguagesId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageStudySubject_StudySubjects_StudySubjectId",
                        column: x => x.StudySubjectId,
                        principalTable: "StudySubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageStudySubject_StudySubjectId",
                table: "LanguageStudySubject",
                column: "StudySubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId",
                principalTable: "InstitutionHierarchies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudySubjects_Languages_PrimaryLanguageId",
                table: "StudySubjects",
                column: "PrimaryLanguageId",
                principalTable: "Languages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
