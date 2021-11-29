using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AddedWorkshopImages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagesMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    StorageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagesMetadata", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "WorkshopImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ImageId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopImages", x => new { x.Id, x.ImageId });
                    table.ForeignKey(
                        name: "FK_WorkshopImages_ImagesMetadata_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ImagesMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopImages_Workshops_Id",
                        column: x => x.Id,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopImages_ImageId",
                table: "WorkshopImages",
                column: "ImageId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkshopImages");

            migrationBuilder.DropTable(
                name: "ImagesMetadata");
        }
    }
}
