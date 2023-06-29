using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddChildSocialGroups : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Children_SocialGroups_SocialGroupId",
            table: "Children");

        migrationBuilder.DropIndex(
            name: "IX_Children_SocialGroupId",
            table: "Children");

        migrationBuilder.DropColumn(
            name: "SocialGroupId",
            table: "Children");

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

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ChildrenSocialGroups");

        migrationBuilder.AddColumn<long>(
            name: "SocialGroupId",
            table: "Children",
            type: "bigint",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Children_SocialGroupId",
            table: "Children",
            column: "SocialGroupId");

        migrationBuilder.AddForeignKey(
            name: "FK_Children_SocialGroups_SocialGroupId",
            table: "Children",
            column: "SocialGroupId",
            principalTable: "SocialGroups",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}