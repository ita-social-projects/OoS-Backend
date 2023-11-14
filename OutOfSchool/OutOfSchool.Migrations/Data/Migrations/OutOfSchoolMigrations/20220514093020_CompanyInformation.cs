using Microsoft.EntityFrameworkCore.Migrations;

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class CompanyInformation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AboutPortalItems");

        migrationBuilder.DropTable(
            name: "SupportInformation");

        migrationBuilder.DropTable(
            name: "AboutPortal");

        migrationBuilder.CreateTable(
                name: "CompanyInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInformation", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompanyInformationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyInformationId = table.Column<Guid>(type: "binary(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInformationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyInformationItems_CompanyInformation_CompanyInformatio~",
                        column: x => x.CompanyInformationId,
                        principalTable: "CompanyInformation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformationItems_CompanyInformationId",
            table: "CompanyInformationItems",
            column: "CompanyInformationId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CompanyInformationItems");

        migrationBuilder.DropTable(
            name: "CompanyInformation");

        migrationBuilder.CreateTable(
                name: "AboutPortal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutPortal", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "SupportInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Description = table.Column<string>(type: "varchar(3000)", maxLength: 3000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportInformation", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AboutPortalItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    AboutPortalId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutPortalItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AboutPortalItems_AboutPortal_AboutPortalId",
                        column: x => x.AboutPortalId,
                        principalTable: "AboutPortal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_AboutPortalItems_AboutPortalId",
            table: "AboutPortalItems",
            column: "AboutPortalId");
    }
}