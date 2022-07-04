using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class MinistryAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinistryAdmins",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstitutionId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    CodeficatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinistryAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinistryAdmins_Codeficators_CodeficatorId",
                        column: x => x.CodeficatorId,
                        principalTable: "Codeficators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MinistryAdmins_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "de\n\r !()+4325>=<?HGIFPQ[]\\rp");

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 5L, "institution admin permissions", "e\n26HGIFPQ[\\", "InstitutionAdmin" });

            migrationBuilder.CreateIndex(
                name: "IX_MinistryAdmins_CodeficatorId",
                table: "MinistryAdmins",
                column: "CodeficatorId");

            migrationBuilder.CreateIndex(
                name: "IX_MinistryAdmins_InstitutionId",
                table: "MinistryAdmins",
                column: "InstitutionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinistryAdmins");

            migrationBuilder.DeleteData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "PackedPermissions",
                value: "de\n\r !()+4325>=<?HGIFPQ[]\\");
        }
    }
}
