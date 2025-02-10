using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAtFieldsForExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Institutions",
                type: "datetime(6)",
                nullable: true,
                defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "InstitutionHierarchies",
                type: "datetime(6)",
                nullable: true,
                defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Directions",
                type: "datetime(6)",
                nullable: true,
                defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "InstitutionHierarchies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Directions");
        }
    }
}
