using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddJudgeModelAndUpdateCompetitiveEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEventAccountingTypes_CompetitiveEvents_Competitiv~",
                table: "CompetitiveEventAccountingTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventDescriptionItems_IsDeleted",
                table: "CompetitiveEventDescriptionItems");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEventAccountingTypes_CompetitiveEventId",
                table: "CompetitiveEventAccountingTypes");

            migrationBuilder.DropColumn(
                name: "ChiefJudgeId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CompetitiveEventDescriptionItems");

            migrationBuilder.DropColumn(
                name: "CompetitiveEventId",
                table: "CompetitiveEventAccountingTypes");

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChildParticipantId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.AlterColumn<long>(
                name: "CategoryId",
                table: "CompetitiveEvents",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<Guid>(
                name: "BuildingHoldingId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.AddColumn<string>(
                name: "Benefits",
                table: "CompetitiveEvents",
                type: "varchar(2000)",
                maxLength: 2000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CompetitiveEventAccountingTypeId",
                table: "CompetitiveEvents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VenueName",
                table: "CompetitiveEvents",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitiveEventId",
                table: "CompetitiveEventDescriptionItems",
                type: "binary(16)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "binary(16)");

            migrationBuilder.CreateTable(
                name: "Judges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    FirstName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsChiefJudge = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompetitiveEventId = table.Column<Guid>(type: "binary(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Judges_CompetitiveEvents_CompetitiveEventId",
                        column: x => x.CompetitiveEventId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_CompetitiveEventAccountingTypeId",
                table: "CompetitiveEvents",
                column: "CompetitiveEventAccountingTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Judges_CompetitiveEventId",
                table: "Judges",
                column: "CompetitiveEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEventAccountingTypes_Competitiv~",
                table: "CompetitiveEvents",
                column: "CompetitiveEventAccountingTypeId",
                principalTable: "CompetitiveEventAccountingTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                table: "CompetitiveEvents",
                column: "ParentId",
                principalTable: "CompetitiveEvents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents",
                column: "CategoryId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                column: "OrganizerOfTheEventId",
                principalTable: "Providers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEventAccountingTypes_Competitiv~",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents");

            migrationBuilder.DropTable(
                name: "Judges");

            migrationBuilder.DropIndex(
                name: "IX_CompetitiveEvents_CompetitiveEventAccountingTypeId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "Benefits",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CompetitiveEventAccountingTypeId",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "VenueName",
                table: "CompetitiveEvents");

            migrationBuilder.AlterColumn<Guid>(
                name: "VenueId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ChildParticipantId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CategoryId",
                table: "CompetitiveEvents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "BuildingHoldingId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChiefJudgeId",
                table: "CompetitiveEvents",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitiveEventId",
                table: "CompetitiveEventDescriptionItems",
                type: "binary(16)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "binary(16)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CompetitiveEventDescriptionItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitiveEventId",
                table: "CompetitiveEventAccountingTypes",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventAccountingTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventAccountingTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventAccountingTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.UpdateData(
                table: "CompetitiveEventAccountingTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "CompetitiveEventId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventDescriptionItems_IsDeleted",
                table: "CompetitiveEventDescriptionItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEventAccountingTypes_CompetitiveEventId",
                table: "CompetitiveEventAccountingTypes",
                column: "CompetitiveEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEventAccountingTypes_CompetitiveEvents_Competitiv~",
                table: "CompetitiveEventAccountingTypes",
                column: "CompetitiveEventId",
                principalTable: "CompetitiveEvents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                table: "CompetitiveEvents",
                column: "ParentId",
                principalTable: "CompetitiveEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Directions_CategoryId",
                table: "CompetitiveEvents",
                column: "CategoryId",
                principalTable: "Directions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                table: "CompetitiveEvents",
                column: "OrganizerOfTheEventId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
