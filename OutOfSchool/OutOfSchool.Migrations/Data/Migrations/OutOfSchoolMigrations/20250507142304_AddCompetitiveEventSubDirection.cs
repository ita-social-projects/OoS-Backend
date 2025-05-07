using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddCompetitiveEventSubDirection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEvents_InstitutionHierarchyId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "InstitutionHierarchyId",
                table: "CompetitiveEvents");

            migrationBuilder.CreateTable(
                name: "CompetitiveEventSubDirection",
                columns: table => new
                {
                    CompetitiveEventId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    SubDirectionsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventSubDirection", x => new { x.CompetitiveEventId, x.SubDirectionsId });
                    table.ForeignKey(
                        name: "FK_CompetitiveEventSubDirection_CompetitiveEvents_CompetitiveEv~",
                        column: x => x.CompetitiveEventId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitiveEventSubDirection_SubDirections_SubDirectionsId",
                        column: x => x.SubDirectionsId,
                        principalTable: "SubDirections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventSubDirection_SubDirectionsId",
                table: "CompetitiveEventSubDirection",
                column: "SubDirectionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitiveEventSubDirection");

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionHierarchyId",
                table: "CompetitiveEvents",
                type: "UUID(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_InstitutionHierarchyId",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId",
                principalTable: "InstitutionHierarchies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
