using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class InstitutionAdminAddFKToUser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "InstitutionAdmins",
            type: "varchar(255)",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "longtext",
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionAdmins_UserId",
            table: "InstitutionAdmins",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_InstitutionAdmins_AspNetUsers_UserId",
            table: "InstitutionAdmins",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_InstitutionAdmins_AspNetUsers_UserId",
            table: "InstitutionAdmins");

        migrationBuilder.DropIndex(
            name: "IX_InstitutionAdmins_UserId",
            table: "InstitutionAdmins");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "InstitutionAdmins",
            type: "longtext",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "varchar(255)",
            oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }
}
