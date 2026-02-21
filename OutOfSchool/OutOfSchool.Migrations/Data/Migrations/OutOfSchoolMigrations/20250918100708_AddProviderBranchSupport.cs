using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddProviderBranchSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentProviderId",
                table: "Providers",
                type: "UUID(16)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EdrpouUniqKey",
                table: "Providers",
                type: "varchar(255)",
                nullable: true,
                computedColumnSql: "\r\n                CASE \r\n                    WHEN `IsStructuralUnit` = 1 \r\n                    THEN CONCAT(`Edrpou`, '-', REPLACE(LOWER(`Id`), '-', ''))\r\n                    ELSE `Edrpou`\r\n                END",
                stored: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_EdrpouUniqKey",
                table: "Providers",
                column: "EdrpouUniqKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Providers_ParentProviderId",
                table: "Providers",
                column: "ParentProviderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_Providers_ParentProviderId",
                table: "Providers",
                column: "ParentProviderId",
                principalTable: "Providers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_Providers_ParentProviderId",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_EdrpouUniqKey",
                table: "Providers");

            migrationBuilder.DropIndex(
                name: "IX_Providers_ParentProviderId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "EdrpouUniqKey",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "ParentProviderId",
                table: "Providers");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_Edrpou",
                table: "Providers",
                column: "Edrpou",
                unique: true);
        }
    }
}
