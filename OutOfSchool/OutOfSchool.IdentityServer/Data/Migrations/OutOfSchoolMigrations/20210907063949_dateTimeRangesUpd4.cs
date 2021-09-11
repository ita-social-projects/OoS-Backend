using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class dateTimeRangesUpd4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges");

            migrationBuilder.DropColumn(
                name: "DaysPerWeek",
                table: "Workshops");

            migrationBuilder.AlterColumn<long>(
                name: "WorkshopId",
                table: "DateTimeRanges",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges");

            migrationBuilder.AddColumn<int>(
                name: "DaysPerWeek",
                table: "Workshops",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "WorkshopId",
                table: "DateTimeRanges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
