using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddressAddCodeficatorId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<long>(
            name: "CodeficatorId",
            table: "Addresses",
            type: "bigint",
            nullable: false,
            defaultValue: 4970L);

        migrationBuilder.CreateIndex(
            name: "IX_Addresses_CodeficatorId",
            table: "Addresses",
            column: "CodeficatorId");

        migrationBuilder.AddForeignKey(
            name: "FK_Addresses_Codeficators_CodeficatorId",
            table: "Addresses",
            column: "CodeficatorId",
            principalTable: "Codeficators",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Addresses_Codeficators_CodeficatorId",
            table: "Addresses");

        migrationBuilder.DropIndex(
            name: "IX_Addresses_CodeficatorId",
            table: "Addresses");

        migrationBuilder.DropColumn(
            name: "CodeficatorId",
            table: "Addresses");
    }
}
