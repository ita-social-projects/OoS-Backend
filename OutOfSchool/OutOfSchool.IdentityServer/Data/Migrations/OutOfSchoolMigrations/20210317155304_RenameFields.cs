using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class RenameFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children");

            migrationBuilder.DropForeignKey(
                name: "FK_Children_SocialGroup_SocialGroupId",
                table: "Children");

            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Address_AddressId",
                table: "Workshops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialGroup",
                table: "SocialGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Address",
                table: "Address");

            migrationBuilder.RenameTable(
                name: "SocialGroup",
                newName: "SocialGroups");

            migrationBuilder.RenameTable(
                name: "Address",
                newName: "Addresses");

            migrationBuilder.RenameColumn(
                name: "MiddleName",
                table: "Children",
                newName: "Patronymic");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Workshops",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "SocialGroupId",
                table: "Children",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ParentId",
                table: "Children",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLogin",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialGroups",
                table: "SocialGroups",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children",
                column: "SocialGroupId",
                principalTable: "SocialGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Children_SocialGroups_SocialGroupId",
                table: "Children");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Addresses_AddressId",
                table: "Workshops");
            
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_Providers_ProviderId",
                table: "Workshops");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SocialGroups",
                table: "SocialGroups");
            
            migrationBuilder.DropPrimaryKey(
                name: "PK_Addresses",
                table: "Addresses");
            
            migrationBuilder.RenameTable(
                name: "SocialGroups",
                newName: "SocialGroup");
            
            migrationBuilder.RenameTable(
                name: "Addresses",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "Patronymic",
                table: "Children",
                newName: "MiddleName");
            
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Workshops",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
            
            migrationBuilder.AlterColumn<long>(
                name: "SocialGroupId",
                table: "Children",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
            
            migrationBuilder.AlterColumn<long>(
                name: "ParentId",
                table: "Children",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
            
            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLogin",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_SocialGroup",
                table: "SocialGroup",
                column: "Id");
            
            migrationBuilder.AddPrimaryKey(
                name: "PK_Address",
                table: "Address",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Children_Parents_ParentId",
                table: "Children",
                column: "ParentId",
                principalTable: "Parents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.AddForeignKey(
                name: "FK_Children_SocialGroup_SocialGroupId",
                table: "Children",
                column: "SocialGroupId",
                principalTable: "SocialGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            
            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_Address_AddressId",
                table: "Workshops",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}