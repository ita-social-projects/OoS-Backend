using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class ChangePhoneNumberFormat : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "Workshops",
            type: "varchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(15)",
            oldMaxLength: 15)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "Providers",
            type: "varchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(15)",
            oldMaxLength: 15)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
            name: "BlockPhoneNumber",
            table: "Providers",
            type: "varchar(16)",
            maxLength: 16,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(15)",
            oldMaxLength: 15)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "Workshops",
            type: "varchar(15)",
            maxLength: 15,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(16)",
            oldMaxLength: 16)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
            name: "PhoneNumber",
            table: "Providers",
            type: "varchar(15)",
            maxLength: 15,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(16)",
            oldMaxLength: 16)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AlterColumn<string>(
            name: "BlockPhoneNumber",
            table: "Providers",
            type: "varchar(15)",
            maxLength: 15,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "varchar(16)",
            oldMaxLength: 16)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");
    }
}
