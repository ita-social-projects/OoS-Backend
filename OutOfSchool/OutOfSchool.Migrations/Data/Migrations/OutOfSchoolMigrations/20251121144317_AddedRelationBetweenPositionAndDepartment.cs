using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddedRelationBetweenPositionAndDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassifierType",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "Rate",
                table: "Positions",
                newName: "TotalRatesForPosition");

            migrationBuilder.AddColumn<Guid>(
                name: "DepartmentId",
                table: "Positions",
                type: "UUID(16)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOffStaffPosition",
                table: "Positions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPedagogicalPosition",
                table: "Positions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PositionClassificationType",
                table: "Positions",
                type: "UUID(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PositionOpenedByOrganization",
                table: "Positions",
                type: "UUID(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<float>(
                name: "PositionRate",
                table: "Positions",
                type: "float",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions",
                column: "DepartmentId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Positions_SeatsAmount_NonNegative",
                table: "Positions",
                sql: "SeatsAmount >= 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Departments_DepartmentId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_DepartmentId",
                table: "Positions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Positions_SeatsAmount_NonNegative",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "IsOffStaffPosition",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "IsPedagogicalPosition",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionClassificationType",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionOpenedByOrganization",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionRate",
                table: "Positions");

            migrationBuilder.RenameColumn(
                name: "TotalRatesForPosition",
                table: "Positions",
                newName: "Rate");

            migrationBuilder.AddColumn<string>(
                name: "ClassifierType",
                table: "Positions",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
