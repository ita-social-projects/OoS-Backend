using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AboutPortalAndSupportInformation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InformationAboutPortal");

            migrationBuilder.DropColumn(
                name: "SectionName",
                table: "InformationAboutPortal");

            migrationBuilder.AddColumn<ulong>(
                name: "GeoHash",
                table: "Cities",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateTable(
                name: "InformationAboutPortalItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InformationAboutPortalId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationAboutPortalItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InformationAboutPortalItem_InformationAboutPortal_Informatio~",
                        column: x => x.InformationAboutPortalId,
                        principalTable: "InformationAboutPortal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SupportInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(3000)", maxLength: 3000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportInformation", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_InformationAboutPortalItem_InformationAboutPortalId",
                table: "InformationAboutPortalItem",
                column: "InformationAboutPortalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InformationAboutPortalItem");

            migrationBuilder.DropTable(
                name: "SupportInformation");

            migrationBuilder.DropColumn(
                name: "GeoHash",
                table: "Cities");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InformationAboutPortal",
                type: "varchar(3000)",
                maxLength: 3000,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SectionName",
                table: "InformationAboutPortal",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
