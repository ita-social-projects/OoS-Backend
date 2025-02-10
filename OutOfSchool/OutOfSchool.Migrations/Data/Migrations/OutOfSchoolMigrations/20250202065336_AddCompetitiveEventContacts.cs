#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations
{
    /// <inheritdoc />
    public partial class AddCompetitiveEventContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Email");

            migrationBuilder.DropTable(
                name: "PhoneNumber");

            migrationBuilder.DropTable(
                name: "SocialNetwork");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ActiveFrom",
                table: "CompetitiveEvents",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ActiveTo",
                table: "CompetitiveEvents",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(9999, 12, 31));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CompetitiveEvents",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CompetitiveEvents",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteDate",
                table: "CompetitiveEvents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "CompetitiveEvents",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Document",
                table: "CompetitiveEvents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "File",
                table: "CompetitiveEvents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "CompetitiveEvents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemProtected",
                table: "CompetitiveEvents",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                table: "CompetitiveEvents",
                type: "char(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CompetitiveEvents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CompetitiveEvents_Contacts",
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
                    OwnerId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEvents_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_Contacts_CATOTTGs_Address_CATOTTGId",
                        column: x => x.Address_CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_Contacts_CompetitiveEvents_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workshops_Contacts",
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
                    OwnerId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workshops_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workshops_Contacts_CATOTTGs_Address_CATOTTGId",
                        column: x => x.Address_CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workshops_Contacts_Workshops_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitiveEvents_Contacts_Emails",
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
                    table.PrimaryKey("PK_CompetitiveEvents_Contacts_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_Contacts_Emails_CompetitiveEvents_Contacts~",
                        column: x => x.ContactsId,
                        principalTable: "CompetitiveEvents_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitiveEvents_Contacts_Phones",
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
                    table.PrimaryKey("PK_CompetitiveEvents_Contacts_Phones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_Contacts_Phones_CompetitiveEvents_Contacts~",
                        column: x => x.ContactsId,
                        principalTable: "CompetitiveEvents_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CompetitiveEvents_Contacts_SocialNetworks",
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
                    table.PrimaryKey("PK_CompetitiveEvents_Contacts_SocialNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_Contacts_SocialNetworks_CompetitiveEvents_~",
                        column: x => x.ContactsId,
                        principalTable: "CompetitiveEvents_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workshops_Contacts_Emails",
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
                    table.PrimaryKey("PK_Workshops_Contacts_Emails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workshops_Contacts_Emails_Workshops_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Workshops_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workshops_Contacts_Phones",
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
                    table.PrimaryKey("PK_Workshops_Contacts_Phones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workshops_Contacts_Phones_Workshops_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Workshops_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Workshops_Contacts_SocialNetworks",
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
                    table.PrimaryKey("PK_Workshops_Contacts_SocialNetworks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workshops_Contacts_SocialNetworks_Workshops_Contacts_Contact~",
                        column: x => x.ContactsId,
                        principalTable: "Workshops_Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_Address_CATOTTGId",
                table: "CompetitiveEvents_Contacts",
                column: "Address_CATOTTGId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_OwnerId",
                table: "CompetitiveEvents_Contacts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_Emails_Address",
                table: "CompetitiveEvents_Contacts_Emails",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_Emails_ContactsId",
                table: "CompetitiveEvents_Contacts_Emails",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_Phones_ContactsId",
                table: "CompetitiveEvents_Contacts_Phones",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_Phones_Number",
                table: "CompetitiveEvents_Contacts_Phones",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_CompetitiveEvents_Contacts_SocialNetworks_ContactsId",
                table: "CompetitiveEvents_Contacts_SocialNetworks",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_Address_CATOTTGId",
                table: "Workshops_Contacts",
                column: "Address_CATOTTGId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_OwnerId",
                table: "Workshops_Contacts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_Emails_Address",
                table: "Workshops_Contacts_Emails",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_Emails_ContactsId",
                table: "Workshops_Contacts_Emails",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_Phones_ContactsId",
                table: "Workshops_Contacts_Phones",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_Phones_Number",
                table: "Workshops_Contacts_Phones",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Workshops_Contacts_SocialNetworks_ContactsId",
                table: "Workshops_Contacts_SocialNetworks",
                column: "ContactsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompetitiveEvents_Contacts_Emails");

            migrationBuilder.DropTable(
                name: "CompetitiveEvents_Contacts_Phones");

            migrationBuilder.DropTable(
                name: "CompetitiveEvents_Contacts_SocialNetworks");

            migrationBuilder.DropTable(
                name: "Workshops_Contacts_Emails");

            migrationBuilder.DropTable(
                name: "Workshops_Contacts_Phones");

            migrationBuilder.DropTable(
                name: "Workshops_Contacts_SocialNetworks");

            migrationBuilder.DropTable(
                name: "CompetitiveEvents_Contacts");

            migrationBuilder.DropTable(
                name: "Workshops_Contacts");

            migrationBuilder.DropColumn(
                name: "ActiveFrom",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "ActiveTo",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "DeleteDate",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "Document",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "File",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "IsSystemProtected",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "CompetitiveEvents");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CompetitiveEvents");

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address_CATOTTGId = table.Column<long>(type: "bigint", nullable: true),
                    Address_BuildingNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address_GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    Address_Latitude = table.Column<double>(type: "double", nullable: true),
                    Address_Longitude = table.Column<double>(type: "double", nullable: true),
                    Address_Street = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contacts_CATOTTGs_Address_CATOTTGId",
                        column: x => x.Address_CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Contacts_Workshops_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Email",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Address = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Email", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Email_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PhoneNumber",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false),
                    Number = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhoneNumber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhoneNumber_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SocialNetwork",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContactsId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialNetwork", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialNetwork_Contacts_ContactsId",
                        column: x => x.ContactsId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Address_CATOTTGId",
                table: "Contacts",
                column: "Address_CATOTTGId");

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_OwnerId",
                table: "Contacts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Email_Address",
                table: "Email",
                column: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Email_ContactsId",
                table: "Email",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumber_ContactsId",
                table: "PhoneNumber",
                column: "ContactsId");

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumber_Number",
                table: "PhoneNumber",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_SocialNetwork_ContactsId",
                table: "SocialNetwork",
                column: "ContactsId");
        }
    }
}
