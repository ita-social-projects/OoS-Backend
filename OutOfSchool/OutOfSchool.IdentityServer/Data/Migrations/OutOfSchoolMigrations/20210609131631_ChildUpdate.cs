using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    /// <summary>
    /// Child migration with updates.
    /// </summary>
    public partial class ChildUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_Addresses_AddressId",
                table: "Children");

            migrationBuilder.DropForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children");

            migrationBuilder.DropIndex(
                name: "IX_Children_AddressId",
                table: "Children");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Children");

            migrationBuilder.RenameColumn(
                name: "Patronymic",
                table: "Children",
                newName: "MiddleName");

            migrationBuilder.AlterColumn<long>(
                name: "SocialGroupId",
                table: "Children",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Children",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Children",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children",
                column: "SocialGroupId",
                principalTable: "SocialGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "Children",
                newName: "Patronymic");

            migrationBuilder.AlterColumn<long>(
                name: "SocialGroupId",
                table: "Children",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Children",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Children",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "Children",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Children_AddressId",
                table: "Children",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Addresses_AddressId",
                table: "Children",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children",
                column: "SocialGroupId",
                principalTable: "SocialGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
