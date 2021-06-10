using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class DateOfBirthRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Workshops_WorkshopId",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "HeadBirthDate",
                table: "Workshops",
                newName: "HeadDateOfBirth");

            migrationBuilder.RenameColumn(
                name: "BirthDay",
                table: "Teachers",
                newName: "DateOfBirth");

            migrationBuilder.RenameColumn(
                name: "DirectorBirthDay",
                table: "Providers",
                newName: "DirectorDateOfBirth");

            migrationBuilder.AlterColumn<long>(
                name: "WorkshopId",
                table: "Teachers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Workshops_WorkshopId",
                table: "Teachers",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Workshops_WorkshopId",
                table: "Teachers");

            migrationBuilder.RenameColumn(
                name: "HeadDateOfBirth",
                table: "Workshops",
                newName: "HeadBirthDate");

            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "Teachers",
                newName: "BirthDay");

            migrationBuilder.RenameColumn(
                name: "DirectorDateOfBirth",
                table: "Providers",
                newName: "DirectorBirthDay");

            migrationBuilder.AlterColumn<long>(
                name: "WorkshopId",
                table: "Teachers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Workshops_WorkshopId",
                table: "Teachers",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
