using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AddPictures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PictureMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StorageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PictureMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderPictureTable",
                columns: table => new
                {
                    ProviderId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderPictureTable", x => new { x.ProviderId, x.PictureId });
                    table.ForeignKey(
                        name: "FK_ProviderPictureTable_PictureMetadata_PictureId",
                        column: x => x.PictureId,
                        principalTable: "PictureMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderPictureTable_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeacherPictureTable",
                columns: table => new
                {
                    TeacherId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherPictureTable", x => new { x.TeacherId, x.PictureId });
                    table.ForeignKey(
                        name: "FK_TeacherPictureTable_PictureMetadata_PictureId",
                        column: x => x.PictureId,
                        principalTable: "PictureMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherPictureTable_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkshopPictureTable",
                columns: table => new
                {
                    WorkshopId = table.Column<long>(type: "bigint", nullable: false),
                    PictureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopPictureTable", x => new { x.WorkshopId, x.PictureId });
                    table.ForeignKey(
                        name: "FK_WorkshopPictureTable_PictureMetadata_PictureId",
                        column: x => x.PictureId,
                        principalTable: "PictureMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopPictureTable_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderPictureTable_PictureId",
                table: "ProviderPictureTable",
                column: "PictureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPictureTable_PictureId",
                table: "TeacherPictureTable",
                column: "PictureId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkshopPictureTable_PictureId",
                table: "WorkshopPictureTable",
                column: "PictureId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderPictureTable");

            migrationBuilder.DropTable(
                name: "TeacherPictureTable");

            migrationBuilder.DropTable(
                name: "WorkshopPictureTable");

            migrationBuilder.DropTable(
                name: "PictureMetadata");
        }
    }
}
