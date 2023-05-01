using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class RemoveWorkshopHeadFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Head",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "HeadDateOfBirth",
            table: "Workshops");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
                name: "Head",
                table: "Workshops",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "HeadDateOfBirth",
            table: "Workshops",
            type: "date",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
    }
}