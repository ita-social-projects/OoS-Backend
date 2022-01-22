using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ProviderAdsmins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDerived",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ProviderAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    IsDeputy = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAdmins", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ProviderAdmins_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ProviderAdminWorkshop",
                columns: table => new
                {
                    ManagedWorkshopsId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ProviderAdminsUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAdminWorkshop", x => new { x.ManagedWorkshopsId, x.ProviderAdminsUserId });
                    table.ForeignKey(
                        name: "FK_ProviderAdminWorkshop_ProviderAdmins_ProviderAdminsUserId",
                        column: x => x.ProviderAdminsUserId,
                        principalTable: "ProviderAdmins",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProviderAdminWorkshop_Workshops_ManagedWorkshopsId",
                        column: x => x.ManagedWorkshopsId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "e\n43256HGIFPQ[]\\");

            migrationBuilder.InsertData(
                table: "PermissionsForRoles",
                columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
                values: new object[] { 4L, "provider admin permissions", "e\n6HGIFPQ[\\", "ProviderAdmin" });

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdmins_ProviderId",
                table: "ProviderAdmins",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdminWorkshop_ProviderAdminsUserId",
                table: "ProviderAdminWorkshop",
                column: "ProviderAdminsUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProviderAdminWorkshop");

            migrationBuilder.DropTable(
                name: "ProviderAdmins");

            migrationBuilder.DeleteData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DropColumn(
                name: "IsDerived",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "e\n4325HGIFPQ[]\\");
        }
    }
}
