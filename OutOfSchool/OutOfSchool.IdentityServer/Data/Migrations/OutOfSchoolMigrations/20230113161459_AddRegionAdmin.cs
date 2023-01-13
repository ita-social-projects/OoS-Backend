using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddRegionAdmin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RegionAdmins",
            columns: table => new
            {
                UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                InstitutionId = table.Column<Guid>(type: "binary(16)", nullable: false),
                CATOTTGId = table.Column<long>(type: "bigint", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RegionAdmins", x => x.UserId);
                table.ForeignKey(
                    name: "FK_RegionAdmins_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RegionAdmins_CATOTTGs_CATOTTGId",
                    column: x => x.CATOTTGId,
                    principalTable: "CATOTTGs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RegionAdmins_Institutions_InstitutionId",
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
            value: "def\n\r !()+43257>=<?HGIFPQ[]\\rpqon|z{yx}T");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n257(PQFTn[zxy{}");

        migrationBuilder.InsertData(
            table: "PermissionsForRoles",
            columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
            values: new object[] { 6L, "region admin permissions", "ef\n257(PQFTy[", "RegionAdmin" });

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_CATOTTGId",
            table: "RegionAdmins",
            column: "CATOTTGId");

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_InstitutionId",
            table: "RegionAdmins",
            column: "InstitutionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RegionAdmins");

        migrationBuilder.DeleteData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L);

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "de\n\r !()+43257>=<?HGIFPQ[]\\rpqoT");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ef\n257(PQFTn[");
    }
}
