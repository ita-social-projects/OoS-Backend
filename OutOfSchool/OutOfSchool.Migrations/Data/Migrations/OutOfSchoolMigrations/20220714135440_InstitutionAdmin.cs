using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class InstitutionAdmin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                name: "InstitutionAdmins",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstitutionId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionAdmins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionAdmins_Institutions_InstitutionId",
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
            value: "de\n\r !()+4325>=<?HGIFPQ[]\\rpqo");

        migrationBuilder.InsertData(
            table: "PermissionsForRoles",
            columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
            values: new object[] { 5L, "ministry admin permissions", "e\n26HGIFPQ[\\", "MinistryAdmin" });

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionAdmins_InstitutionId",
            table: "InstitutionAdmins",
            column: "InstitutionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "InstitutionAdmins");

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
            
        migrationBuilder.DeleteData(
            table: "AspNetRoles",
            keyColumn: "Id",
            keyValue: "9995f142-9a03-4cc5-b806-c52fd6932760");
    }
}