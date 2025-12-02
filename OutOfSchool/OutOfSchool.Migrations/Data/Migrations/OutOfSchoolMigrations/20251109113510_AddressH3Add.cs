using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddressH3Add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MinsportSectionId",
                table: "WorkshopDrafts",
                type: "UUID(16)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrySyncDate",
                table: "InstitutionHierarchies",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinsportSectionId",
                table: "WorkshopDrafts");

            migrationBuilder.DropColumn(
                name: "RegistrySyncDate",
                table: "InstitutionHierarchies");
        }
    }
}
