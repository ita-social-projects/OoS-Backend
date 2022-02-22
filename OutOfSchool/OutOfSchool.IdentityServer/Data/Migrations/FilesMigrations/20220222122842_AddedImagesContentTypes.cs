using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.FilesMigrations
{
    public partial class AddedImagesContentTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Images");

            migrationBuilder.AddColumn<int>(
                name: "ContentTypeId",
                table: "Images",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ImageContentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContentTypeValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageContentTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "ImageContentTypes",
                columns: new[] { "Id", "ContentTypeValue" },
                values: new object[] { 1, "image/jpeg" });

            migrationBuilder.InsertData(
                table: "ImageContentTypes",
                columns: new[] { "Id", "ContentTypeValue" },
                values: new object[] { 2, "image/png" });

            migrationBuilder.CreateIndex(
                name: "IX_Images_ContentTypeId",
                table: "Images",
                column: "ContentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_ImageContentTypes_ContentTypeId",
                table: "Images",
                column: "ContentTypeId",
                principalTable: "ImageContentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_ImageContentTypes_ContentTypeId",
                table: "Images");

            migrationBuilder.DropTable(
                name: "ImageContentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Images_ContentTypeId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "ContentTypeId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Images",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
