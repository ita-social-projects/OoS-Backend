using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class ProviderType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Providers");

            migrationBuilder.AddColumn<long>(
                name: "TypeId",
                table: "Providers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ProviderTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderTypes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Providers_TypeId",
                table: "Providers",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Providers_ProviderTypes_TypeId",
                table: "Providers",
                column: "TypeId",
                principalTable: "ProviderTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Providers_ProviderTypes_TypeId",
                table: "Providers");

            migrationBuilder.DropTable(
                name: "ProviderTypes");

            migrationBuilder.DropIndex(
                name: "IX_Providers_TypeId",
                table: "Providers");

            migrationBuilder.DropColumn(
                name: "TypeId",
                table: "Providers");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Providers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
