using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class TimeRanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "Workshops");

            migrationBuilder.CreateTable(
                name: "DateTimeRanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkshopId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateTimeRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DateTimeRanges_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workdays",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateTimeRangeId = table.Column<long>(type: "bigint", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                        column: x => x.DateTimeRangeId,
                        principalTable: "DateTimeRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DateTimeRanges_WorkshopId",
                table: "DateTimeRanges",
                column: "WorkshopId");

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_DateTimeRangeId",
                table: "Workdays",
                column: "DateTimeRangeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Workdays");

            migrationBuilder.DropTable(
                name: "DateTimeRanges");

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "Workshops",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
