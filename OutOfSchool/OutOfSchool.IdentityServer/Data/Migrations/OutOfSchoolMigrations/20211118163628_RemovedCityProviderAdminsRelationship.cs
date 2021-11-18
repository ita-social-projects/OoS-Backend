using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class RemovedCityProviderAdminsRelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProviderAdmins_Cities_CityId",
                table: "ProviderAdmins");

            migrationBuilder.DropIndex(
                name: "IX_ProviderAdmins_CityId",
                table: "ProviderAdmins");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ProviderAdmins");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CityId",
                table: "ProviderAdmins",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAdmins_CityId",
                table: "ProviderAdmins",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProviderAdmins_Cities_CityId",
                table: "ProviderAdmins",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
