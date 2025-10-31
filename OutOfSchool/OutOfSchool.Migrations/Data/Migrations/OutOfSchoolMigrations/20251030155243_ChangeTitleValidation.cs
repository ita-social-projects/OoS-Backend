#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class ChangeTitleValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Workshops_Contacts_SocialNetworks",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Workshops",
                type: "varchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(120)",
                oldMaxLength: 120)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Providers_Contacts_SocialNetworks",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CompetitiveEvents_Contacts_SocialNetworks",
                type: "varchar(2048)",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EdrpouUniqKey",
                table: "Providers",
                type: "varchar(255)",
                nullable: true,
                computedColumnSql: "\n                CASE \n                    WHEN `IsStructuralUnit` = 1 \n                    THEN CONCAT(`Edrpou`, '-', REPLACE(LOWER(`Id`), '-', ''))\n                    ELSE `Edrpou`\n                END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true,
                oldComputedColumnSql: "\r\n                CASE \r\n                    WHEN `IsStructuralUnit` = 1 \r\n                    THEN CONCAT(`Edrpou`, '-', REPLACE(LOWER(`Id`), '-', ''))\r\n                    ELSE `Edrpou`\r\n                END",
                oldStored: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Workshops_Contacts_SocialNetworks",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Workshops",
                type: "varchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldMaxLength: 250)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Providers_Contacts_SocialNetworks",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "CompetitiveEvents_Contacts_SocialNetworks",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(2048)",
                oldMaxLength: 2048,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "EdrpouUniqKey",
                table: "Providers",
                type: "varchar(255)",
                nullable: true,
                computedColumnSql: "\r\n                CASE \r\n                    WHEN `IsStructuralUnit` = 1 \r\n                    THEN CONCAT(`Edrpou`, '-', REPLACE(LOWER(`Id`), '-', ''))\r\n                    ELSE `Edrpou`\r\n                END",
                stored: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true,
                oldComputedColumnSql: "\n                CASE \n                    WHEN `IsStructuralUnit` = 1 \n                    THEN CONCAT(`Edrpou`, '-', REPLACE(LOWER(`Id`), '-', ''))\n                    ELSE `Edrpou`\n                END",
                oldStored: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
