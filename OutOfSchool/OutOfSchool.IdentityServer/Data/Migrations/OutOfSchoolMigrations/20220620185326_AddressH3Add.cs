using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddressH3Add : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Description",
            table: "Workshops");

        migrationBuilder.CreateTable(
                name: "WorkshopDescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopDescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopDescriptionItems_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDescriptionItems_WorkshopId",
            table: "WorkshopDescriptionItems",
            column: "WorkshopId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "WorkshopDescriptionItems");

        migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Workshops",
                type: "varchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");
    }
}