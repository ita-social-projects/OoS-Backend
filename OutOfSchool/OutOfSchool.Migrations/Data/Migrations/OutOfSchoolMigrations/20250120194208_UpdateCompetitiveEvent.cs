using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class UpdateCompetitiveEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEvents_CategoryId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "NumberOfRatings",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "Subcategory",
                table: "CompetitiveEvents");

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

            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionHierarchyId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TermsOfParticipation",
                table: "CompetitiveEvents",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_InstitutionHierarchyId",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                table: "CompetitiveEvents",
                column: "InstitutionHierarchyId",
                principalTable: "InstitutionHierarchies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "TermsOfParticipation",
                table: "CompetitiveEvents");

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

            migrationBuilder.AddColumn<long>(
                name: "CategoryId",
                table: "CompetitiveEvents",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "NumberOfRatings",
                table: "CompetitiveEvents",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "Rating",
                table: "CompetitiveEvents",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "Subcategory",
                table: "CompetitiveEvents",
                type: "varchar(250)",
                maxLength: 250,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_CategoryId",
                table: "CompetitiveEvents",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents",
                column: "CategoryId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
