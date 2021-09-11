using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class dateTimeRangesUpd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRange_Workshops_WorkshopId",
                table: "DateTimeRange");

            migrationBuilder.DropForeignKey(
                name: "FK_Workday_DateTimeRange_DateTimeRangeId",
                table: "Workday");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workday",
                table: "Workday");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DateTimeRange",
                table: "DateTimeRange");

            migrationBuilder.RenameTable(
                name: "Workday",
                newName: "Workdays");

            migrationBuilder.RenameTable(
                name: "DateTimeRange",
                newName: "DateTimeRanges");

            migrationBuilder.RenameIndex(
                name: "IX_Workday_DateTimeRangeId",
                table: "Workdays",
                newName: "IX_Workdays_DateTimeRangeId");

            migrationBuilder.RenameIndex(
                name: "IX_DateTimeRange_WorkshopId",
                table: "DateTimeRanges",
                newName: "IX_DateTimeRanges_WorkshopId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workdays",
                table: "Workdays",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DateTimeRanges",
                table: "DateTimeRanges",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays",
                column: "DateTimeRangeId",
                principalTable: "DateTimeRanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Workdays",
                table: "Workdays");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DateTimeRanges",
                table: "DateTimeRanges");

            migrationBuilder.RenameTable(
                name: "Workdays",
                newName: "Workday");

            migrationBuilder.RenameTable(
                name: "DateTimeRanges",
                newName: "DateTimeRange");

            migrationBuilder.RenameIndex(
                name: "IX_Workdays_DateTimeRangeId",
                table: "Workday",
                newName: "IX_Workday_DateTimeRangeId");

            migrationBuilder.RenameIndex(
                name: "IX_DateTimeRanges_WorkshopId",
                table: "DateTimeRange",
                newName: "IX_DateTimeRange_WorkshopId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Workday",
                table: "Workday",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DateTimeRange",
                table: "DateTimeRange",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DateTimeRange_Workshops_WorkshopId",
                table: "DateTimeRange",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workday_DateTimeRange_DateTimeRangeId",
                table: "Workday",
                column: "DateTimeRangeId",
                principalTable: "DateTimeRange",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
