using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ChildSocialGroupTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildrenSocialGroups");

            migrationBuilder.CreateTable(
                name: "ChildSocialGroup",
                columns: table => new
                {
                    ChildrenId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SocialGroupsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildSocialGroup", x => new { x.ChildrenId, x.SocialGroupsId });
                    table.ForeignKey(
                        name: "FK_ChildSocialGroup_Children_ChildrenId",
                        column: x => x.ChildrenId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildSocialGroup_SocialGroups_SocialGroupsId",
                        column: x => x.SocialGroupsId,
                        principalTable: "SocialGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChildSocialGroup_SocialGroupsId",
                table: "ChildSocialGroup",
                column: "SocialGroupsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChildSocialGroup");

            migrationBuilder.CreateTable(
                name: "ChildrenSocialGroups",
                columns: table => new
                {
                    ChildId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SocialGroupId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildrenSocialGroups", x => new { x.ChildId, x.SocialGroupId });
                    table.ForeignKey(
                        name: "FK_ChildrenSocialGroups_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildrenSocialGroups_SocialGroups_SocialGroupId",
                        column: x => x.SocialGroupId,
                        principalTable: "SocialGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChildrenSocialGroups_SocialGroupId",
                table: "ChildrenSocialGroups",
                column: "SocialGroupId");
        }
    }
}
