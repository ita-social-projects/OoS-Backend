using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class TeacherImages : Migration
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

            migrationBuilder.CreateTable(
                name: "TeacherImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_TeacherImages_Teachers_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeacherImages");

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
}
