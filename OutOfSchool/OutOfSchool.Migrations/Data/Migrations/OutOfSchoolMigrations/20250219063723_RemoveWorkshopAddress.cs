#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class RemoveWorkshopAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_AddressId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Facebook",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Instagram",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Workshops");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "Workshops",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Workshops",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Facebook",
                table: "Workshops",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Instagram",
                table: "Workshops",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Workshops",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Workshops",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_AddressId",
                table: "Workshops",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
