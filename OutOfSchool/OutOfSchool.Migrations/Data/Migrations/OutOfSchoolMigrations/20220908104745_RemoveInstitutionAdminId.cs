using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class RemoveInstitutionAdminId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_InstitutionAdmins_AspNetUsers_UserId",
            table: "InstitutionAdmins");

        migrationBuilder.DropPrimaryKey(
            name: "PK_InstitutionAdmins",
            table: "InstitutionAdmins");

        migrationBuilder.DropIndex(
            name: "IX_InstitutionAdmins_UserId",
            table: "InstitutionAdmins");

        migrationBuilder.DropColumn(
            name: "Id",
            table: "InstitutionAdmins");

        migrationBuilder.UpdateData(
            table: "InstitutionAdmins",
            keyColumn: "UserId",
            keyValue: null,
            column: "UserId",
            value: "");

        migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "InstitutionAdmins",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddPrimaryKey(
            name: "PK_InstitutionAdmins",
            table: "InstitutionAdmins",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_InstitutionAdmins_AspNetUsers_UserId",
            table: "InstitutionAdmins",
            column: "UserId",
            principalTable: "AspNetUsers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_InstitutionAdmins_AspNetUsers_UserId",
            table: "InstitutionAdmins");

        migrationBuilder.DropPrimaryKey(
            name: "PK_InstitutionAdmins",
            table: "InstitutionAdmins");

        migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "InstitutionAdmins",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "InstitutionAdmins",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

        migrationBuilder.AddPrimaryKey(
            name: "PK_InstitutionAdmins",
            table: "InstitutionAdmins",
            column: "Id");

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
}