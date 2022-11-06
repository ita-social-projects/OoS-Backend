using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations
{
    public partial class StatisticReports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilesInDb",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ContentType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Data = table.Column<byte[]>(type: "longblob", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesInDb", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StatisticReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    ReportDataType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalStorageId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticReports", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StatisticReportsCSV",
                columns: table => new
                {
                    Year = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    ProviderName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EDRPOU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstitutionTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CATOTTGCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Region = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TerritorialCommunity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Settlement = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CATOTTGCategory = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Complex = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopsAmount = table.Column<int>(type: "int", nullable: false),
                    ApplicationsAmount = table.Column<int>(type: "int", nullable: false),
                    ApplicationsApproved = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudying = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingFemale = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingLess18 = table.Column<int>(type: "int", nullable: false),
                    Teachers = table.Column<int>(type: "int", nullable: false),
                    WorkshopsAmountInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingFemaleInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingAchievementsInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingLargeFamilyInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingPoorFamilyInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingDisabilityInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    ChildrenStudyingOrphanInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersInstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersLess30InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersFrom31To40InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersFrom41To50InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersFrom51To55InstitutionHierarchy = table.Column<int>(type: "int", nullable: false),
                    TeachersFrom55InstitutionHierarchy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilesInDb");

            migrationBuilder.DropTable(
                name: "StatisticReports");

            migrationBuilder.DropTable(
                name: "StatisticReportsCSV");
        }
    }
}
