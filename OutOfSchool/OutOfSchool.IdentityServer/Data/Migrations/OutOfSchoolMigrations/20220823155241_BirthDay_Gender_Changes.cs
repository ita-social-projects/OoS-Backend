using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class BirthDay_Gender_Changes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Gender",
            table: "AspNetUsers");

        migrationBuilder.AddColumn<DateTime>(
            name: "DateOfBirth",
            table: "Parents",
            type: "date",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<int>(
            name: "Gender",
            table: "Parents",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DateOfBirth",
            table: "Parents");

        migrationBuilder.DropColumn(
            name: "Gender",
            table: "Parents");

        migrationBuilder.AddColumn<int>(
            name: "Gender",
            table: "AspNetUsers",
            type: "int",
            nullable: false,
            defaultValue: 0);
    }
}