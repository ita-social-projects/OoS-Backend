using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class AddBirthCertificateTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BirthCertificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    SvidSer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidNum = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidNumMD5 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidWho = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SvidDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BirthCertificates");
        }
    }
}
