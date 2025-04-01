#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class RefactorProviderEntity : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Providers_Addresses_ActualAddressId",
            table: "Providers");

        migrationBuilder.DropForeignKey(
            name: "FK_Providers_Addresses_LegalAddressId",
            table: "Providers");

        migrationBuilder.DropIndex(
            name: "IX_Providers_ActualAddressId",
            table: "Providers");

        migrationBuilder.DropIndex(
            name: "IX_Providers_EdrpouIpn",
            table: "Providers");

        migrationBuilder.DropIndex(
            name: "IX_Providers_LegalAddressId",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ActualAddressId",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Director",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "DirectorDateOfBirth",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "EdrpouIpn",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Email",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Founder",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "LegalAddressId",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "PhoneNumber",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Website",
            table: "Providers");

        migrationBuilder.RenameColumn(
            name: "Instagram",
            table: "Providers",
            newName: "LicenseLimits");

        migrationBuilder.RenameColumn(
            name: "Facebook",
            table: "Providers",
            newName: "InstitutionCode");

        migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Providers",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)")
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "Providers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateOnly>(
            name: "ActiveFrom",
            table: "Providers",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(1, 1, 1));

        migrationBuilder.AddColumn<DateOnly>(
            name: "ActiveTo",
            table: "Providers",
            type: "date",
            nullable: false,
            defaultValue: new DateOnly(9999, 12, 31));

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "Providers",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Providers",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "DeleteDate",
            table: "Providers",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Providers",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Document",
                table: "Providers",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Edrpou",
                table: "Providers",
                type: "varchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<Guid>(
            name: "ExternalId",
            table: "Providers",
            type: "binary(16)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
                name: "File",
                table: "Providers",
                type: "longtext",
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "GeneralWorkSchedule",
                table: "Providers",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<bool>(
            name: "IsLocatedInMountainousArea",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsStructuralUnit",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsSystemProtected",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LicenseExpirationDate",
            table: "Providers",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "LicenseIssuanceDate",
            table: "Providers",
            type: "datetime(6)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "Providers",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<bool>(
            name: "UsesOutsourcingServices",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.CreateTable(
                name: "Providers_Contacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Address_Street = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address_BuildingNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address_Latitude = table.Column<double>(type: "double", nullable: true),
                    Address_Longitude = table.Column<double>(type: "double", nullable: true),
                    Address_GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Address_CATOTTGId = table.Column<long>(type: "bigint", nullable: true),
                    OwnerId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Contacts_CATOTTGs_Address_CATOTTGId",
                        column: x => x.Address_CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Providers_Contacts_Providers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Providers_Contacts_Emails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers_Contacts_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Contacts_Emails_Providers_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Providers_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Providers_Contacts_Phones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers_Contacts_Phones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Contacts_Phones_Providers_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Providers_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Providers_Contacts_SocialNetworks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers_Contacts_SocialNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Contacts_SocialNetworks_Providers_Contacts_Contact~",
                        column: x => x.ContactsId,
                        principalTable: "Providers_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Edrpou",
            table: "Providers",
            column: "Edrpou");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_Address_CATOTTGId",
            table: "Providers_Contacts",
            column: "Address_CATOTTGId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_OwnerId",
            table: "Providers_Contacts",
            column: "OwnerId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_Emails_Address",
            table: "Providers_Contacts_Emails",
            column: "Address");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_Emails_ContactsId",
            table: "Providers_Contacts_Emails",
            column: "ContactsId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_Phones_ContactsId",
            table: "Providers_Contacts_Phones",
            column: "ContactsId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_Phones_Number",
            table: "Providers_Contacts_Phones",
            column: "Number");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_Contacts_SocialNetworks_ContactsId",
            table: "Providers_Contacts_SocialNetworks",
            column: "ContactsId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Providers_Contacts_Emails");

        migrationBuilder.DropTable(
            name: "Providers_Contacts_Phones");

        migrationBuilder.DropTable(
            name: "Providers_Contacts_SocialNetworks");

        migrationBuilder.DropTable(
            name: "Providers_Contacts");

        migrationBuilder.DropIndex(
            name: "IX_Providers_Edrpou",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ActiveFrom",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ActiveTo",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "CreatedAt",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "DeleteDate",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "DeletedBy",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Document",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "Edrpou",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ExternalId",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "File",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "GeneralWorkSchedule",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "IsLocatedInMountainousArea",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "IsStructuralUnit",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "IsSystemProtected",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "LicenseExpirationDate",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "LicenseIssuanceDate",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "ModifiedBy",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "UsesOutsourcingServices",
            table: "Providers");

        migrationBuilder.RenameColumn(
            name: "LicenseLimits",
            table: "Providers",
            newName: "Instagram");

        migrationBuilder.RenameColumn(
            name: "InstitutionCode",
            table: "Providers",
            newName: "Facebook");

        migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Providers",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
            .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

        migrationBuilder.AlterColumn<string>(
                name: "CoverImageId",
                table: "Providers",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
            .Annotation("MySql:CharSet", "utf8mb4")
            .OldAnnotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<long>(
            name: "ActualAddressId",
            table: "Providers",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<string>(
                name: "Director",
                table: "Providers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<DateTime>(
            name: "DirectorDateOfBirth",
            table: "Providers",
            type: "Date",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

        migrationBuilder.AddColumn<string>(
                name: "EdrpouIpn",
                table: "Providers",
                type: "varchar(12)",
                maxLength: 12,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Providers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Founder",
                table: "Providers",
                type: "varchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<long>(
            name: "LegalAddressId",
            table: "Providers",
            type: "bigint",
            nullable: false,
            defaultValue: 0L);

        migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Providers",
                type: "varchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "")
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Providers",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_ActualAddressId",
            table: "Providers",
            column: "ActualAddressId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Providers_EdrpouIpn",
            table: "Providers",
            column: "EdrpouIpn");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_LegalAddressId",
            table: "Providers",
            column: "LegalAddressId",
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Providers_Addresses_ActualAddressId",
            table: "Providers",
            column: "ActualAddressId",
            principalTable: "Addresses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Providers_Addresses_LegalAddressId",
            table: "Providers",
            column: "LegalAddressId",
            principalTable: "Addresses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }
}