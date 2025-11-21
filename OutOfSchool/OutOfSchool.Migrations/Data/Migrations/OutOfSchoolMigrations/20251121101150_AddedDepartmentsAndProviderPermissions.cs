using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddedDepartmentsAndProviderPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GenitiveName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Abbreviation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentDepartmentId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    DepartmentType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParticipantId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ParentOrganizationId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    EducationProcessForm = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EducationalDirections = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EducationLevelProvided = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizationSpecialization = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HasConsultationUnit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GeneralSchedule = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdditionalDescription = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperationalStatus = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsLocatedInMountains = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsBranchUnit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ZpoType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Document = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    File = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ActiveTo = table.Column<DateOnly>(type: "date", nullable: false, defaultValue: new DateOnly(9999, 12, 31)),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSystemProtected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DeletedBy = table.Column<string>(type: "char(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DeleteDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Departments_Contacts",
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
                    table.PrimaryKey("PK_Departments_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Contacts_CATOTTGs_Address_CATOTTGId",
                        column: x => x.Address_CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Departments_Contacts_Departments_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Departments_Contacts_Emails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments_Contacts_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Contacts_Emails_Departments_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Departments_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Departments_Contacts_Phones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Number = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments_Contacts_Phones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Contacts_Phones_Departments_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Departments_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Departments_Contacts_SocialNetworks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments_Contacts_SocialNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Contacts_SocialNetworks_Departments_Contacts_Con~",
                        column: x => x.ContactsId,
                        principalTable: "Departments_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5mgoaOipKWmpw==");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_IsDeleted",
                table: "Departments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_Address_CATOTTGId",
                table: "Departments_Contacts",
                column: "Address_CATOTTGId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_OwnerId",
                table: "Departments_Contacts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_Emails_Address",
                table: "Departments_Contacts_Emails",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_Emails_ContactsId",
                table: "Departments_Contacts_Emails",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_Phones_ContactsId",
                table: "Departments_Contacts_Phones",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_Phones_Number",
                table: "Departments_Contacts_Phones",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Contacts_SocialNetworks_ContactsId",
                table: "Departments_Contacts_SocialNetworks",
                column: "ContactsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Departments_Contacts_Emails");

            migrationBuilder.DropTable(
                name: "Departments_Contacts_Phones");

            migrationBuilder.DropTable(
                name: "Departments_Contacts_SocialNetworks");

            migrationBuilder.DropTable(
                name: "Departments_Contacts");

            migrationBuilder.DropTable(
                name: "Departments");

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

            migrationBuilder.UpdateData(
                table: "PermissionsForRoles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "PackedPermissions",
                value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5mgoaOi");
        }
    }
}
