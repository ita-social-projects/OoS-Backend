using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class dateTimeRangesUpd2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays");

            migrationBuilder.AlterColumn<long>(
                name: "DateTimeRangeId",
                table: "Workdays",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays",
                column: "DateTimeRangeId",
                principalTable: "DateTimeRanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DateTimeRanges_Workshops_WorkshopId",
                table: "DateTimeRanges");

            migrationBuilder.DropForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays");

            migrationBuilder.AlterColumn<long>(
                name: "DateTimeRangeId",
                table: "Workdays",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Workdays_DateTimeRanges_DateTimeRangeId",
                table: "Workdays",
                column: "DateTimeRangeId",
                principalTable: "DateTimeRanges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
