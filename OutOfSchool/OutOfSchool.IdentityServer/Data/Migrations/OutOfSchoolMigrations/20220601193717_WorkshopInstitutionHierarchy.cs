using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class WorkshopInstitutionHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstitutionHierarchyId",
                table: "Workshops",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_InstitutionHierarchyId",
                table: "Workshops",
                column: "InstitutionHierarchyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_InstitutionHierarchies_InstitutionHierarchyId",
                table: "Workshops",
                column: "InstitutionHierarchyId",
                principalTable: "InstitutionHierarchies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_InstitutionHierarchies_InstitutionHierarchyId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_InstitutionHierarchyId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "InstitutionHierarchyId",
                table: "Workshops");
        }
    }
}
