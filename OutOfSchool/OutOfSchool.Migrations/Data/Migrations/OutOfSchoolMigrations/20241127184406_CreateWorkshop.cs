using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class CreateWorkshop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalDescription",
                table: "Workshops",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AreThereBenefits",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "CategoryId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "CoverageId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultTeacherId",
                table: "Workshops",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EducationalDisciplines",
                table: "Workshops",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<uint>(
                name: "EducationalShiftId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "GropeTypeId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<bool>(
                name: "IsInclusive",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfFinanced",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSpecial",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "LanguageOfEducationId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<Guid>(
                name: "MemberOfWorkshopId",
                table: "Workshops",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferentialTermsOfParticipation",
                table: "Workshops",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ShortStay",
                table: "Workshops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<uint>(
                name: "SpecialNeedsId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<uint>(
                name: "TypeOfAgeCompositionId",
                table: "Workshops",
                type: "int unsigned",
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkshopId",
                table: "Teachers",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_DefaultTeacherId",
                table: "Workshops",
                column: "DefaultTeacherId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_MemberOfWorkshopId",
                table: "Workshops",
                column: "MemberOfWorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Teachers_DefaultTeacherId",
                table: "Workshops",
                column: "DefaultTeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Workshops_MemberOfWorkshopId",
                table: "Workshops",
                column: "MemberOfWorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Teachers_DefaultTeacherId",
                table: "Workshops");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Workshops_MemberOfWorkshopId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_DefaultTeacherId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_MemberOfWorkshopId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AdditionalDescription",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AreThereBenefits",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "CoverageId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "DefaultTeacherId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "EducationalDisciplines",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "EducationalShiftId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "GropeTypeId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsInclusive",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsSelfFinanced",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsSpecial",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "LanguageOfEducationId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "MemberOfWorkshopId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "PreferentialTermsOfParticipation",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "ShortStay",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "SpecialNeedsId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "TypeOfAgeCompositionId",
                table: "Workshops");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkshopId",
                table: "Teachers",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);
        }
    }
}
