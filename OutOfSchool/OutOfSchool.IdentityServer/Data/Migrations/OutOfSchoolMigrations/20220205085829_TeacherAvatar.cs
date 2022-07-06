using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class TeacherAvatar : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Image",
            table: "Teachers");

        migrationBuilder.AddColumn<string>(
                name: "AvatarImageId",
                table: "Teachers",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "AvatarImageId",
            table: "Teachers");

        migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Teachers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");
    }
}