using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ChildDeleteBirthCert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BirthCertificates");

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfStudy",
                table: "Children",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOfStudy",
                table: "Children");

            migrationBuilder.CreateTable(
                name: "BirthCertificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SvidDate = table.Column<DateTime>(type: "date", nullable: false),
                    SvidNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidNumMD5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidSer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidWho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirthCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BirthCertificates_Children_Id",
                        column: x => x.Id,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
