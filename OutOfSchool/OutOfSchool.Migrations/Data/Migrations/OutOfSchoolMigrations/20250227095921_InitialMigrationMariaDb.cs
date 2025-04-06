#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class InitialMigrationMariaDb : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterDatabase()
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AchievementTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleEn = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTypes", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatingTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    LastLogin = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRegistered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDerived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MustChangePassword = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "char(84)", unicode: false, fixedLength: true, maxLength: 84, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "varchar(36)", unicode: false, maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AverageRatings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Rate = table.Column<float>(type: "float", nullable: false),
                    RateQuantity = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AverageRatings", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CATOTTGs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    Category = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    NeedCheck = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsTop = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CATOTTGs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CATOTTGs_CATOTTGs_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompanyInformation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
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
                name: "CompetitiveEventAccountingTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleEn = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventAccountingTypes", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompetitiveEventCoverages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleEn = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventCoverages", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompetitiveEventRegistrationDeadlines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TitleEn = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventRegistrationDeadlines", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FriendlyName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Xml = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Directions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directions", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ElasticsearchSyncRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Entity = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    OperationDate = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Operation = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ElasticsearchSyncRecords", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumberOfHierarchyLevels = table.Column<int>(type: "int", nullable: false),
                    IsGovernment = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameEn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionStatuses", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Data = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupedData = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ReadDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    ObjectId = table.Column<Guid>(type: "UUID(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "OperationsWithObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    EntityType = table.Column<int>(type: "int", nullable: true),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    RowSeparator = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsWithObjects", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "PermissionsForRoles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackedPermissions = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissionsForRoles", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ProviderTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderTypes", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "QuartzJobs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastSuccessLaunch = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuartzJobs", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "SocialGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameEn = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialGroups", x => x.Id);
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
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
                    table.PrimaryKey("PK_StatisticReportsCSV", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameEn = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EntityType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EntityIdGuid = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    EntityIdLong = table.Column<long>(type: "bigint", nullable: true),
                    OldValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChangesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangesLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsRegistered = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rnokpp = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ExternalRegistryId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: true)
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
                    table.PrimaryKey("PK_Individuals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Individuals_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Parents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Street = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BuildingNumber = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<double>(type: "double", nullable: false),
                    Longitude = table.Column<double>(type: "double", nullable: false),
                    GeoHash = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CATOTTGId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_CATOTTGs_CATOTTGId",
                        column: x => x.CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompanyInformationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompanyInformationId = table.Column<Guid>(type: "UUID(16)", nullable: false)
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

        migrationBuilder.CreateTable(
                name: "AreaAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CATOTTGId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaAdmins", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_AreaAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaAdmins_CATOTTGs_CATOTTGId",
                        column: x => x.CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaAdmins_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionAdmins", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_InstitutionAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionAdmins_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionFieldDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionFieldDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionFieldDescriptions_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "InstitutionHierarchies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    HierarchyLevel = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "NULL ON UPDATE CURRENT_TIMESTAMP(6)"),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionHierarchies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchies_InstitutionHierarchies_ParentId",
                        column: x => x.ParentId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InstitutionHierarchies_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "RegionAdmins",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CATOTTGId = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionAdmins", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_RegionAdmins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionAdmins_CATOTTGs_CATOTTGId",
                        column: x => x.CATOTTGId,
                        principalTable: "CATOTTGs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionAdmins_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "StudySubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    NameInUkrainian = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameInInstructionLanguage = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsLanguageUkrainian = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LanguageId = table.Column<long>(type: "bigint", nullable: false),
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
                    table.PrimaryKey("PK_StudySubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySubjects_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Children",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PlaceOfStudy = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsParent = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Children", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Children_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ParentBlockedByAdminLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentBlockedByAdminLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParentBlockedByAdminLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ParentBlockedByAdminLog_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    CreationTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ratings_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "DirectionInstitutionHierarchy",
                columns: table => new
                {
                    DirectionsId = table.Column<long>(type: "bigint", nullable: false),
                    InstitutionHierarchiesId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectionInstitutionHierarchy", x => new { x.DirectionsId, x.InstitutionHierarchiesId });
                    table.ForeignKey(
                        name: "FK_DirectionInstitutionHierarchy_Directions_DirectionsId",
                        column: x => x.DirectionsId,
                        principalTable: "Directions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DirectionInstitutionHierarchy_InstitutionHierarchies_Institu~",
                        column: x => x.InstitutionHierarchiesId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ChildSocialGroup",
                columns: table => new
                {
                    ChildrenId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    SocialGroupsId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChildSocialGroup", x => new { x.ChildrenId, x.SocialGroupsId });
                    table.ForeignKey(
                        name: "FK_ChildSocialGroup_Children_ChildrenId",
                        column: x => x.ChildrenId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChildSocialGroup_SocialGroups_SocialGroupsId",
                        column: x => x.SocialGroupsId,
                        principalTable: "SocialGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AchievementChild",
                columns: table => new
                {
                    AchievementsId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ChildrenId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementChild", x => new { x.AchievementsId, x.ChildrenId });
                    table.ForeignKey(
                        name: "FK_AchievementChild_Children_ChildrenId",
                        column: x => x.ChildrenId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Title = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AchievementDate = table.Column<DateTime>(type: "date", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    AchievementTypeId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_AchievementTypes_AchievementTypeId",
                        column: x => x.AchievementTypeId,
                        principalTable: "AchievementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "AchievementTeachers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    AchievementId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementTeachers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementTeachers_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    RejectionMessage = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreationTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ApprovedTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    IsBlockedByProvider = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ChildId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Children_ChildId",
                        column: x => x.ChildId,
                        principalTable: "Children",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Applications_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "BlockedProviderParents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserIdBlock = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserIdUnblock = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateTimeFrom = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    DateTimeTo = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedProviderParents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedProviderParents_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ChatMessageWorkshops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ChatRoomId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Text = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SenderRoleIsProvider = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ReadDateTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageWorkshops", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ChatRoomWorkshops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsBlockedByProvider = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRoomWorkshops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRoomWorkshops_Parents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Parents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompetitiveEventDescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompetitiveEventId = table.Column<Guid>(type: "UUID(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompetitiveEventDescriptionItems", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "CompetitiveEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortTitle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<int>(type: "int", nullable: false),
                    RegistrationStartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    RegistrationEndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ParentId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    BuildingHoldingId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    ChildParticipantId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    AdditionalDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ScheduledStartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ScheduledEndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    NumberOfSeats = table.Column<uint>(type: "int unsigned", nullable: false),
                    CompetitiveEventAccountingTypeId = table.Column<int>(type: "int", nullable: false),
                    CoverageId = table.Column<int>(type: "int", nullable: false),
                    DescriptionOfTheEnrollmentProcedure = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OrganizerOfTheEventId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    PlannedFormatOfClasses = table.Column<int>(type: "int", nullable: false),
                    VenueId = table.Column<Guid>(type: "binary(16)", nullable: true),
                    VenueName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TermsOfParticipation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PreferentialTermsOfParticipation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreThereBenefits = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Benefits = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OptionsForPeopleWithDisabilities = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DescriptionOfOptionsForPeopleWithDisabilities = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstitutionHierarchyId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    MinimumAge = table.Column<int>(type: "int", nullable: false),
                    MaximumAge = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<int>(type: "int", nullable: false),
                    CompetitiveSelection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    NumberOfOccupiedSeats = table.Column<uint>(type: "int unsigned", nullable: false),
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
                    table.PrimaryKey("PK_CompetitiveEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_CompetitiveEventAccountingTypes_Competitiv~",
                        column: x => x.CompetitiveEventAccountingTypeId,
                        principalTable: "CompetitiveEventAccountingTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_CompetitiveEventCoverages_CoverageId",
                        column: x => x.CoverageId,
                        principalTable: "CompetitiveEventCoverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                        column: x => x.ParentId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompetitiveEvents_InstitutionHierarchies_InstitutionHierarch~",
                        column: x => x.InstitutionHierarchyId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

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
                    OwnerId = table.Column<Guid>(type: "UUID(16)", nullable: false)
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
                name: "Judges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                    FirstName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsChiefJudge = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompetitiveEventId = table.Column<Guid>(type: "UUID(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Judges_CompetitiveEvents_CompetitiveEventId",
                        column: x => x.CompetitiveEventId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    FullTitle = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortTitle = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullTitleEn = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortTitleEn = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Website = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Facebook = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Instagram = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EdrpouIpn = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Director = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DirectorDateOfBirth = table.Column<DateTime>(type: "Date", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Founder = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ownership = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    StatusReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    License = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LicenseStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    BlockPhoneNumber = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BlockReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActualAddressId = table.Column<long>(type: "bigint", nullable: true),
                    LegalAddressId = table.Column<long>(type: "bigint", nullable: false),
                    InstitutionStatusId = table.Column<long>(type: "bigint", nullable: true),
                    InstitutionId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    InstitutionType = table.Column<int>(type: "int", nullable: false),
                    CoverImageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn),
                    CompetitiveEventId = table.Column<Guid>(type: "UUID(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Providers_Addresses_ActualAddressId",
                        column: x => x.ActualAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Providers_Addresses_LegalAddressId",
                        column: x => x.LegalAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Providers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Providers_CompetitiveEvents_CompetitiveEventId",
                        column: x => x.CompetitiveEventId,
                        principalTable: "CompetitiveEvents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Providers_InstitutionStatuses_InstitutionStatusId",
                        column: x => x.InstitutionStatusId,
                        principalTable: "InstitutionStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Providers_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Providers_ProviderTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ProviderTypes",
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
                name: "EmployeeChangesLog",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeChangesLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_AspNetUsers_EmployeeUserId",
                        column: x => x.EmployeeUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeChangesLog_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    BlockingType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Employees_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Positions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Language = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsForRuralAreas = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Department = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ContactId = table.Column<Guid>(type: "binary(16)", nullable: false),
                    SeatsAmount = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GenitiveName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsTeachingPosition = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Rate = table.Column<float>(type: "float", nullable: false),
                    Tariff = table.Column<float>(type: "float", nullable: false),
                    ClassifierType = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
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
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ProviderImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_ProviderImages_Providers_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "ProviderSectionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderSectionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProviderSectionItems_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Officials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    PositionId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IndividualId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    DismissalOrder = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RecruitmentOrder = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DismissalReason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    ExternalRegistryId = table.Column<Guid>(type: "binary(16)", nullable: false),
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
                    table.PrimaryKey("PK_Officials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Officials_Individuals_IndividualId",
                        column: x => x.IndividualId,
                        principalTable: "Individuals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Officials_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "DateTimeRanges",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    StartTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Workdays = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateTimeRanges", x => x.Id);
                    table.CheckConstraint("CK_DateTimeRanges_EndTimeIsAfterStartTime", "EndTime >= StartTime");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "EmployeeWorkshop",
                columns: table => new
                {
                    EmployeesUserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ManagedWorkshopsId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWorkshop", x => new { x.EmployeesUserId, x.ManagedWorkshopsId });
                    table.ForeignKey(
                        name: "FK_EmployeeWorkshop_Employees_EmployeesUserId",
                        column: x => x.EmployeesUserId,
                        principalTable: "Employees",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favorites_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "TagWorkshop",
                columns: table => new
                {
                    TagsId = table.Column<long>(type: "bigint", nullable: false),
                    WorkshopsId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagWorkshop", x => new { x.TagsId, x.WorkshopsId });
                    table.ForeignKey(
                        name: "FK_TagWorkshop_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "TeacherDraft",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageId = table.Column<string>(type: "char(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopDraftId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDefaultTeacher = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDraft", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Teachers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MiddleName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImageId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teachers", x => x.Id);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "Workshops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    Title = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShortTitle = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinAge = table.Column<int>(type: "int", nullable: false),
                    MaxAge = table.Column<int>(type: "int", nullable: false),
                    AvailableSeats = table.Column<uint>(type: "int unsigned", nullable: false),
                    CompetitiveSelection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WorkshopType = table.Column<int>(type: "int", nullable: false),
                    AgeComposition = table.Column<int>(type: "int", nullable: false),
                    EducationalShift = table.Column<int>(type: "int", nullable: false),
                    ShortStay = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSelfFinanced = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FormOfLearning = table.Column<int>(type: "int", nullable: false),
                    IsPaid = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LanguageOfEducationId = table.Column<uint>(type: "int unsigned", nullable: false),
                    CompetitiveSelectionDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderTitle = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderTitleEn = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderOwnership = table.Column<int>(type: "int", nullable: false),
                    Keywords = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayRate = table.Column<int>(type: "int", nullable: false),
                    WithDisabilityOptions = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DisabilityOptionsDesc = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsSpecial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SpecialNeedsType = table.Column<int>(type: "int", nullable: false),
                    IsInclusive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EnrollmentProcedureDescription = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreThereBenefits = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PreferentialTermsOfParticipation = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Coverage = table.Column<int>(type: "int", nullable: false),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    InstitutionHierarchyId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    DefaultTeacherId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    CoverImageId = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentWorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: true),
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
                    table.PrimaryKey("PK_Workshops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workshops_InstitutionHierarchies_InstitutionHierarchyId",
                        column: x => x.InstitutionHierarchyId,
                        principalTable: "InstitutionHierarchies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Workshops_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Workshops_Teachers_DefaultTeacherId",
                        column: x => x.DefaultTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Workshops_Workshops_ParentWorkshopId",
                        column: x => x.ParentWorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "WorkshopDescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopDescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopDescriptionItems_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "WorkshopDrafts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ProviderId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "UUID(16)", nullable: true),
                    CoverImageId = table.Column<string>(type: "char(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DraftStatus = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Version = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    RejectionMessage = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WorkshopDraftContent = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ModifiedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkshopDrafts_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkshopDrafts_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id");
                })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
                name: "WorkshopImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_WorkshopImages_Workshops_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Workshops",
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
                    OwnerId = table.Column<Guid>(type: "UUID(16)", nullable: false)
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
                name: "WorkshopDraftImages",
                columns: table => new
                {
                    EntityId = table.Column<Guid>(type: "UUID(16)", nullable: false),
                    ExternalStorageId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkshopDraftImages", x => new { x.EntityId, x.ExternalStorageId });
                    table.ForeignKey(
                        name: "FK_WorkshopDraftImages_WorkshopDrafts_EntityId",
                        column: x => x.EntityId,
                        principalTable: "WorkshopDrafts",
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

        migrationBuilder.InsertData(
            table: "AchievementTypes",
            columns: new[] { "Id", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1L, "Переможці міжнародних та всеукраїнських спортивних змагань (індивідуальних та командних)", "Winners of international and all-Ukrainian sports competitions (individual and team)" },
                { 2L, "Призери та учасники міжнародних, всеукраїнських та призери регіональних конкурсів і виставок наукових, технічних, дослідницьких, інноваційних, ІТ проектів", "Winners and participants of international, all-Ukrainian and regional contests and exhibitions of scientific, technical, research, innovation, IT projects" },
                { 3L, "Реципієнти міжнародних грантів", "Recipients of international grants" },
                { 4L, "Призери міжнародних культурних конкурсів та фестивалів", "Winners of international cultural competitions and festivals" },
                { 5L, "Соціально активні категорії учнів", "Socially active categories of students" },
                { 6L, "Цифрові інструменти Google для закладів вищої та фахової передвищої освіти", "Google digital tools for institutions of higher and professional pre-higher education" },
                { 7L, "Переможці та учасники олімпіад міжнародного та всеукраїнського рівнів", "Winners and participants of olympiads at the international and all-Ukrainian levels" }
            });

        migrationBuilder.InsertData(
            table: "CompetitiveEventAccountingTypes",
            columns: new[] { "Id", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, "Освітній проєкт", "Educational project" },
                { 2, "Конкурс (не має етапів)", "Competition" },
                { 3, "Основний конкурс (має мати підпорядковані конкурси-етапи)", "Main competition" },
                { 4, "Етап конкурсу (має мати батьківський основний конкурс)", "Contest stage" }
            });

        migrationBuilder.InsertData(
            table: "CompetitiveEventCoverages",
            columns: new[] { "Id", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, "Локальний (Шкільний)", "Local (School)" },
                { 2, "Міський", "City" },
                { 3, "Районний", "Raional" },
                { 4, "Обласний", "Regional" },
                { 5, "Всеукраїнський", "All-Ukrainian" },
                { 6, "Міжнародний", "International" }
            });

        migrationBuilder.InsertData(
            table: "CompetitiveEventRegistrationDeadlines",
            columns: new[] { "Id", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, "Постійно (протягом року)", "Constantly (during the year)" },
                { 2, "Певний місяць або місяці року", "A certain month or months of the year" }
            });

        migrationBuilder.InsertData(
            table: "InstitutionStatuses",
            columns: new[] { "Id", "Name", "NameEn" },
            values: new object[,]
            {
                { 1L, "Працює", "Active" },
                { 2L, "Перебуває в стані реорганізації", "Undergoing reorganization" },
                { 3L, "Має намір на реорганізацію", "Waiting for reorganization" },
                { 4L, "Відсутній статус", "Without status" }
            });

        migrationBuilder.InsertData(
            table: "Languages",
            columns: new[] { "Id", "Code", "Name" },
            values: new object[,]
            {
                { 1L, "en", "English" },
                { 2L, "uk", "Українська" }
            });

        migrationBuilder.InsertData(
            table: "PermissionsForRoles",
            columns: new[] { "Id", "Description", "PackedPermissions", "RoleName" },
            values: new object[,]
            {
                { 1L, "techadmin permissions", "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11cXnJwcW9ufHp7eXh9kI6PjYyRVGeW", "TechAdmin" },
                { 2L, "provider permissions", "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5mgoaOi", "Provider" },
                { 3L, "parent permissions", "ZQMKCwwUFhUXHiAfISgpKz49PFBRVJY=", "Parent" },
                { 4L, "employee permissions", "ZQMCAQQKCzI2SEdJRlBRW1xUlg==", "Employee" },
                { 5L, "ministry admin permissions", "ZWYDAgEECjI1NzgoLBRQUUZUblteenh5e32OjI2PkWeW", "MinistryAdmin" },
                { 6L, "region admin permissions", "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW15nlg==", "RegionAdmin" },
                { 7L, "area admin permissions", "ZWYDAgEECjI1NzgoFFBRRlSMjVteZ5Y=", "AreaAdmin" },
                { 8L, "moderator permissions", "MjdaXlQ=", "Moderator" }
            });

        migrationBuilder.InsertData(
            table: "ProviderTypes",
            columns: new[] { "Id", "Name" },
            values: new object[,]
            {
                { 1L, "Дитячо-юнацькі спортивні школи: комплексні дитячо-юнацькі спортивні школи, дитячо-юнацькі спортивні школи з видів спорту, дитячо-юнацькі спортивні школи для осіб з інвалідністю, спеціалізовані дитячо-юнацькі школи олімпійського резерву, спеціалізовані дитячо-юнацькі спортивні школи для осіб з інвалідністю паралімпійського та дефлімпійського резерву" },
                { 2L, "Клуби: військово-патріотичного виховання, дитячо-юнацькі (моряків, річковиків, авіаторів, космонавтів, парашутистів, десантників, прикордонників, радистів, пожежників, автолюбителів, краєзнавців, туристів, етнографів, фольклористів, фізичної підготовки та інших напрямів)" },
                { 3L, "Мала академія мистецтв (народних ремесел)" },
                { 4L, "Мала академія наук учнівської молоді" },
                { 5L, "Оздоровчі заклади для дітей та молоді: дитячо-юнацькі табори (містечка, комплекси): оздоровчі, заміські, профільні, праці та відпочинку, санаторного типу, з денним перебуванням; туристські бази" },
                { 6L, "Мистецькі школи: музична, художня, хореографічна, хорова, школа мистецтв тощо" },
                { 7L, "Центр, палац, будинок, клуб художньої творчості дітей, юнацтва та молоді, художньо-естетичної творчості учнівської молоді, дитячої та юнацької творчості, естетичного виховання" },
                { 8L, "Центр, будинок, клуб еколого-натуралістичної творчості учнівської молоді, станція юних натуралістів" },
                { 9L, "Центр, будинок, клуб науково-технічної творчості учнівської молоді, станція юних техніків" },
                { 10L, "Центр, будинок, клуб, бюро туризму, краєзнавства, спорту та екскурсій учнівської молоді, туристсько-краєзнавчої творчості учнівської молоді, станція юних туристів" },
                { 11L, "Центри: військово-патріотичного та інших напрямів позашкільної освіти" },
                { 12L, "Дитяча бібліотека, дитяча флотилія моряків і річковиків, дитячий парк, дитячий стадіон, дитячо-юнацька картинна галерея, дитячо-юнацька студія (хорова, театральна, музична, фольклорна тощо), кімната школяра, курси, студії, школи мистецтв, освітньо-культурні центри національних меншин" },
                { 13L, "Інше" }
            });

        migrationBuilder.InsertData(
            table: "SocialGroups",
            columns: new[] { "Id", "Name", "NameEn" },
            values: new object[,]
            {
                { 1L, "Діти із багатодітних сімей", "Children from large families" },
                { 2L, "Діти із малозабезпечених сімей", "Children from low-income families" },
                { 3L, "Діти з інвалідністю", "Children with disabilities" },
                { 4L, "Діти-сироти", "Orphans" },
                { 5L, "Діти, позбавлені батьківського піклування", "Children deprived of parental care" }
            });

        migrationBuilder.InsertData(
            table: "Tags",
            columns: new[] { "Id", "Name", "NameEn" },
            values: new object[,]
            {
                { 1L, "Музичний Гурток", "Music Workshop" },
                { 2L, "Спортивна Секція", "Sports Section" },
                { 3L, "Хореографія", "Choreography" },
                { 4L, "Образотворче Мистецтво", "Fine Arts" },
                { 5L, "Театральна Студія", "Theater Studio" },
                { 6L, "Футбол", "Football" },
                { 7L, "Волейбол", "Volleyball" },
                { 8L, "Плавання", "Swimming" },
                { 9L, "Легка Атлетика", "Track and Field" },
                { 10L, "Баскетбол", "Basketball" },
                { 11L, "Гімнастика", "Gymnastics" },
                { 12L, "Танці", "Dancing" },
                { 13L, "Йога", "Yoga" },
                { 14L, "Карате", "Karate" },
                { 15L, "Айкідо", "Aikido" },
                { 16L, "Боротьба", "Wrestling" },
                { 17L, "Джудо", "Judo" },
                { 18L, "Кулінарія", "Culinary Arts" },
                { 19L, "Рукоділля", "Handicrafts" },
                { 20L, "Малювання", "Drawing" },
                { 21L, "Скульптура", "Sculpture" },
                { 22L, "Фотографія", "Photography" },
                { 23L, "Кіно Мистецтво", "Cinema Art" },
                { 24L, "Акторська Майстерність", "Acting" },
                { 25L, "Психологічні Тренінги", "Psychological Training" },
                { 26L, "Робототехніка", "Robotics" },
                { 27L, "Програмування", "Programming" },
                { 28L, "Інформаційні Технології", "Information Technology" },
                { 29L, "Шахи", "Chess" },
                { 30L, "Логіка", "Logic" },
                { 31L, "Екологія", "Ecology" },
                { 32L, "Наукові Дослідження", "Scientific Research" },
                { 33L, "Біологія", "Biology" },
                { 34L, "Астрономія", "Astronomy" },
                { 35L, "Математика", "Mathematics" },
                { 36L, "Фізика", "Physics" },
                { 37L, "Хімія", "Chemistry" },
                { 38L, "Іноземні Мови", "Foreign Languages" },
                { 39L, "Англійська Мова", "English Language" },
                { 40L, "Німецька Мова", "German Language" },
                { 41L, "Французька Мова", "French Language" },
                { 42L, "Іспанська Мова", "Spanish Language" },
                { 43L, "Журналістика", "Journalism" },
                { 44L, "Риторика", "Rhetoric" },
                { 45L, "Літературна Творчість", "Literary Creativity" },
                { 46L, "Історія", "History" },
                { 47L, "Археологія", "Archaeology" },
                { 48L, "Мистецтвознавство", "Art Studies" },
                { 49L, "Культурологія", "Cultural Studies" },
                { 50L, "Краєзнавство", "Local History" },
                { 51L, "Етнографія", "Ethnography" },
                { 52L, "Радіо Аматорство", "Radio Amateur" },
                { 53L, "Модельний Спорт", "Model Sports" },
                { 54L, "Авіамоделювання", "Aeromodelling" },
                { 55L, "Судномоделювання", "Ship Modelling" },
                { 56L, "Конструювання", "Construction" },
                { 57L, "Технічне Моделювання", "Technical Modelling" },
                { 58L, "Декоративно Прикладне Мистецтво", "Decorative Arts" },
                { 59L, "Кераміка", "Ceramics" },
                { 60L, "Різьба По Дереву", "Wood Carving" },
                { 61L, "Вишивка", "Embroidery" },
                { 62L, "Плетіння", "Weaving" },
                { 63L, "Бісероплетіння", "Bead Weaving" },
                { 64L, "Флористика", "Floristry" },
                { 65L, "Дизайн", "Design" },
                { 66L, "Архітектура", "Architecture" },
                { 67L, "Моделювання Одягу", "Fashion Design" },
                { 68L, "Кравецтво", "Tailoring" },
                { 69L, "Хенд Мейд", "Handmade" },
                { 70L, "Графічний Дизайн", "Graphic Design" },
                { 71L, "Анімація", "Animation" },
                { 72L, "3D Моделювання", "3D Modelling" },
                { 73L, "Мультиплікація", "Cartoon Making" },
                { 74L, "Відеомонтаж", "Video Editing" },
                { 75L, "Цифровий Мистецький Дизайн", "Digital Art Design" },
                { 76L, "Сучасне Мистецтво", "Modern Art" },
                { 77L, "Естрадний Спів", "Pop Singing" },
                { 78L, "Вокальний Ансамбль", "Vocal Ensemble" },
                { 79L, "Оркестр", "Orchestra" },
                { 80L, "Гра На Гітарі", "Guitar Playing" },
                { 81L, "Гра На Фортепіано", "Piano Playing" },
                { 82L, "Сольний Спів", "Solo Singing" },
                { 83L, "Хоровий Спів", "Choral Singing" },
                { 84L, "Фольклорний Ансамбль", "Folklore Ensemble" },
                { 85L, "Етнічна Музика", "Ethnic Music" },
                { 86L, "Духові Інструменти", "Wind Instruments" },
                { 87L, "Струнні Інструменти", "String Instruments" },
                { 88L, "Барабани", "Drums" },
                { 89L, "Перкусія", "Percussion" },
                { 90L, "Музичний Театр", "Musical Theater" },
                { 91L, "Сценічна Мова", "Stage Speech" },
                { 92L, "Імпровізація", "Improvisation" },
                { 93L, "Сценічний Рух", "Stage Movement" },
                { 94L, "Сценографія", "Scenography" },
                { 95L, "Художнє Читання", "Artistic Reading" },
                { 96L, "Модерн", "Modern Dance" },
                { 97L, "Балет", "Ballet" },
                { 98L, "Сучасні Танці", "Modern Dances" },
                { 99L, "Народні Танці", "Folk Dances" },
                { 100L, "Фітнес", "Fitness" }
            });

        migrationBuilder.CreateIndex(
            name: "IX_AchievementChild_ChildrenId",
            table: "AchievementChild",
            column: "ChildrenId");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_AchievementTypeId",
            table: "Achievements",
            column: "AchievementTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_IsDeleted",
            table: "Achievements",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_WorkshopId",
            table: "Achievements",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTeachers_AchievementId",
            table: "AchievementTeachers",
            column: "AchievementId");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTeachers_IsDeleted",
            table: "AchievementTeachers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTypes_IsDeleted",
            table: "AchievementTypes",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Addresses_CATOTTGId",
            table: "Addresses",
            column: "CATOTTGId");

        migrationBuilder.CreateIndex(
            name: "IX_Addresses_IsDeleted",
            table: "Addresses",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_ChildId",
            table: "Applications",
            column: "ChildId");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_IsDeleted",
            table: "Applications",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_ParentId",
            table: "Applications",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_WorkshopId",
            table: "Applications",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_AreaAdmins_CATOTTGId",
            table: "AreaAdmins",
            column: "CATOTTGId");

        migrationBuilder.CreateIndex(
            name: "IX_AreaAdmins_InstitutionId",
            table: "AreaAdmins",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_AreaAdmins_IsDeleted",
            table: "AreaAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetRoleClaims_RoleId",
            table: "AspNetRoleClaims",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "RoleNameIndex",
            table: "AspNetRoles",
            column: "NormalizedName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserClaims_UserId",
            table: "AspNetUserClaims",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserLogins_UserId",
            table: "AspNetUserLogins",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUserRoles_RoleId",
            table: "AspNetUserRoles",
            column: "RoleId");

        migrationBuilder.CreateIndex(
            name: "EmailIndex",
            table: "AspNetUsers",
            column: "NormalizedEmail");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_IsDeleted",
            table: "AspNetUsers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "UserNameIndex",
            table: "AspNetUsers",
            column: "NormalizedUserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AverageRatings_EntityId",
            table: "AverageRatings",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_AverageRatings_IsDeleted",
            table: "AverageRatings",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_IsDeleted",
            table: "BlockedProviderParents",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_ParentId",
            table: "BlockedProviderParents",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_ProviderId",
            table: "BlockedProviderParents",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_CATOTTGs_IsDeleted",
            table: "CATOTTGs",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CATOTTGs_ParentId",
            table: "CATOTTGs",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ChangesLog_UserId",
            table: "ChangesLog",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessageWorkshops_ChatRoomId",
            table: "ChatMessageWorkshops",
            column: "ChatRoomId");

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessageWorkshops_IsDeleted",
            table: "ChatMessageWorkshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ChatRoomWorkshops_IsDeleted",
            table: "ChatRoomWorkshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ChatRoomWorkshops_ParentId",
            table: "ChatRoomWorkshops",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ChatRoomWorkshops_WorkshopId",
            table: "ChatRoomWorkshops",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_Children_IsDeleted",
            table: "Children",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Children_ParentId",
            table: "Children",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ChildSocialGroup_SocialGroupsId",
            table: "ChildSocialGroup",
            column: "SocialGroupsId");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformationItems_CompanyInformationId",
            table: "CompanyInformationItems",
            column: "CompanyInformationId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventAccountingTypes_IsDeleted",
            table: "CompetitiveEventAccountingTypes",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventCoverages_IsDeleted",
            table: "CompetitiveEventCoverages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventDescriptionItems_CompetitiveEventId",
            table: "CompetitiveEventDescriptionItems",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventRegistrationDeadlines_IsDeleted",
            table: "CompetitiveEventRegistrationDeadlines",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_CompetitiveEventAccountingTypeId",
            table: "CompetitiveEvents",
            column: "CompetitiveEventAccountingTypeId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_CoverageId",
            table: "CompetitiveEvents",
            column: "CoverageId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_InstitutionHierarchyId",
            table: "CompetitiveEvents",
            column: "InstitutionHierarchyId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_IsDeleted",
            table: "CompetitiveEvents",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_OrganizerOfTheEventId",
            table: "CompetitiveEvents",
            column: "OrganizerOfTheEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_ParentId",
            table: "CompetitiveEvents",
            column: "ParentId");

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
            name: "IX_DateTimeRanges_IsDeleted",
            table: "DateTimeRanges",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_DateTimeRanges_WorkshopId",
            table: "DateTimeRanges",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_DirectionInstitutionHierarchy_InstitutionHierarchiesId",
            table: "DirectionInstitutionHierarchy",
            column: "InstitutionHierarchiesId");

        migrationBuilder.CreateIndex(
            name: "IX_Directions_IsDeleted",
            table: "Directions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeChangesLog_EmployeeUserId",
            table: "EmployeeChangesLog",
            column: "EmployeeUserId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeChangesLog_ProviderId",
            table: "EmployeeChangesLog",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeChangesLog_UserId",
            table: "EmployeeChangesLog",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_IsDeleted",
            table: "Employees",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Employees_ProviderId",
            table: "Employees",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_EmployeeWorkshop_ManagedWorkshopsId",
            table: "EmployeeWorkshop",
            column: "ManagedWorkshopsId");

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_IsDeleted",
            table: "Favorites",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_UserId",
            table: "Favorites",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_WorkshopId",
            table: "Favorites",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_Individuals_IsDeleted",
            table: "Individuals",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Individuals_Rnokpp",
            table: "Individuals",
            column: "Rnokpp",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Individuals_UserId",
            table: "Individuals",
            column: "UserId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionAdmins_InstitutionId",
            table: "InstitutionAdmins",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionAdmins_IsDeleted",
            table: "InstitutionAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionFieldDescriptions_InstitutionId",
            table: "InstitutionFieldDescriptions",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionFieldDescriptions_IsDeleted",
            table: "InstitutionFieldDescriptions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_InstitutionId",
            table: "InstitutionHierarchies",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_IsDeleted",
            table: "InstitutionHierarchies",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_ParentId",
            table: "InstitutionHierarchies",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Institutions_IsDeleted",
            table: "Institutions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionStatuses_IsDeleted",
            table: "InstitutionStatuses",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Judges_CompetitiveEventId",
            table: "Judges",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_Languages_Code",
            table: "Languages",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Notifications_UserId",
            table: "Notifications",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Officials_IndividualId",
            table: "Officials",
            column: "IndividualId");

        migrationBuilder.CreateIndex(
            name: "IX_Officials_IsDeleted",
            table: "Officials",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Officials_PositionId",
            table: "Officials",
            column: "PositionId");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_EntityId",
            table: "OperationsWithObjects",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_EntityType",
            table: "OperationsWithObjects",
            column: "EntityType");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_OperationType",
            table: "OperationsWithObjects",
            column: "OperationType");

        migrationBuilder.CreateIndex(
            name: "IX_OperationsWithObjects_RowSeparator",
            table: "OperationsWithObjects",
            column: "RowSeparator");

        migrationBuilder.CreateIndex(
            name: "IX_ParentBlockedByAdminLog_ParentId",
            table: "ParentBlockedByAdminLog",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_ParentBlockedByAdminLog_UserId",
            table: "ParentBlockedByAdminLog",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Parents_IsDeleted",
            table: "Parents",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Parents_UserId",
            table: "Parents",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Positions_IsDeleted",
            table: "Positions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Positions_ProviderId",
            table: "Positions",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_ActualAddressId",
            table: "Providers",
            column: "ActualAddressId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Providers_CompetitiveEventId",
            table: "Providers",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_EdrpouIpn",
            table: "Providers",
            column: "EdrpouIpn");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_InstitutionId",
            table: "Providers",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_InstitutionStatusId",
            table: "Providers",
            column: "InstitutionStatusId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_IsDeleted",
            table: "Providers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_LegalAddressId",
            table: "Providers",
            column: "LegalAddressId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Providers_TypeId",
            table: "Providers",
            column: "TypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_UserId",
            table: "Providers",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_ProviderSectionItems_IsDeleted",
            table: "ProviderSectionItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProviderSectionItems_ProviderId",
            table: "ProviderSectionItems",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_Ratings_EntityId",
            table: "Ratings",
            column: "EntityId");

        migrationBuilder.CreateIndex(
            name: "IX_Ratings_IsDeleted",
            table: "Ratings",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Ratings_ParentId",
            table: "Ratings",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_CATOTTGId",
            table: "RegionAdmins",
            column: "CATOTTGId");

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_InstitutionId",
            table: "RegionAdmins",
            column: "InstitutionId");

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_IsDeleted",
            table: "RegionAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_SocialGroups_IsDeleted",
            table: "SocialGroups",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_StudySubjects_IsDeleted",
            table: "StudySubjects",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_StudySubjects_LanguageId",
            table: "StudySubjects",
            column: "LanguageId");

        migrationBuilder.CreateIndex(
            name: "IX_TagWorkshop_WorkshopsId",
            table: "TagWorkshop",
            column: "WorkshopsId");

        migrationBuilder.CreateIndex(
            name: "IX_TeacherDraft_WorkshopDraftId",
            table: "TeacherDraft",
            column: "WorkshopDraftId");

        migrationBuilder.CreateIndex(
            name: "IX_Teachers_IsDeleted",
            table: "Teachers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Teachers_WorkshopId",
            table: "Teachers",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDescriptionItems_IsDeleted",
            table: "WorkshopDescriptionItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDescriptionItems_WorkshopId",
            table: "WorkshopDescriptionItems",
            column: "WorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDrafts_ProviderId",
            table: "WorkshopDrafts",
            column: "ProviderId");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDrafts_WorkshopId",
            table: "WorkshopDrafts",
            column: "WorkshopId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_DefaultTeacherId",
            table: "Workshops",
            column: "DefaultTeacherId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_InstitutionHierarchyId",
            table: "Workshops",
            column: "InstitutionHierarchyId");

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_IsDeleted",
            table: "Workshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_ParentWorkshopId",
            table: "Workshops",
            column: "ParentWorkshopId");

        migrationBuilder.CreateIndex(
            name: "IX_Workshops_ProviderId",
            table: "Workshops",
            column: "ProviderId");

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

        migrationBuilder.AddForeignKey(
            name: "FK_AchievementChild_Achievements_AchievementsId",
            table: "AchievementChild",
            column: "AchievementsId",
            principalTable: "Achievements",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Achievements_Workshops_WorkshopId",
            table: "Achievements",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Applications_Workshops_WorkshopId",
            table: "Applications",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_BlockedProviderParents_Providers_ProviderId",
            table: "BlockedProviderParents",
            column: "ProviderId",
            principalTable: "Providers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ChatMessageWorkshops_ChatRoomWorkshops_ChatRoomId",
            table: "ChatMessageWorkshops",
            column: "ChatRoomId",
            principalTable: "ChatRoomWorkshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ChatRoomWorkshops_Workshops_WorkshopId",
            table: "ChatRoomWorkshops",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CompetitiveEventDescriptionItems_CompetitiveEvents_Competiti~",
            table: "CompetitiveEventDescriptionItems",
            column: "CompetitiveEventId",
            principalTable: "CompetitiveEvents",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
            table: "CompetitiveEvents",
            column: "OrganizerOfTheEventId",
            principalTable: "Providers",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_DateTimeRanges_Workshops_WorkshopId",
            table: "DateTimeRanges",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_EmployeeWorkshop_Workshops_ManagedWorkshopsId",
            table: "EmployeeWorkshop",
            column: "ManagedWorkshopsId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Favorites_Workshops_WorkshopId",
            table: "Favorites",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TagWorkshop_Workshops_WorkshopsId",
            table: "TagWorkshop",
            column: "WorkshopsId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_TeacherDraft_WorkshopDrafts_WorkshopDraftId",
            table: "TeacherDraft",
            column: "WorkshopDraftId",
            principalTable: "WorkshopDrafts",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Teachers_Workshops_WorkshopId",
            table: "Teachers",
            column: "WorkshopId",
            principalTable: "Workshops",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Teachers_Workshops_WorkshopId",
            table: "Teachers");

        migrationBuilder.DropForeignKey(
            name: "FK_Addresses_CATOTTGs_CATOTTGId",
            table: "Addresses");

        migrationBuilder.DropForeignKey(
            name: "FK_Providers_AspNetUsers_UserId",
            table: "Providers");

        migrationBuilder.DropForeignKey(
            name: "FK_InstitutionHierarchies_Institutions_InstitutionId",
            table: "InstitutionHierarchies");

        migrationBuilder.DropForeignKey(
            name: "FK_Providers_Institutions_InstitutionId",
            table: "Providers");

        migrationBuilder.DropForeignKey(
            name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
            table: "CompetitiveEvents");

        migrationBuilder.DropTable(
            name: "AchievementChild");

        migrationBuilder.DropTable(
            name: "AchievementTeachers");

        migrationBuilder.DropTable(
            name: "Applications");

        migrationBuilder.DropTable(
            name: "AreaAdmins");

        migrationBuilder.DropTable(
            name: "AspNetRoleClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserClaims");

        migrationBuilder.DropTable(
            name: "AspNetUserLogins");

        migrationBuilder.DropTable(
            name: "AspNetUserRoles");

        migrationBuilder.DropTable(
            name: "AspNetUserTokens");

        migrationBuilder.DropTable(
            name: "AverageRatings");

        migrationBuilder.DropTable(
            name: "BlockedProviderParents");

        migrationBuilder.DropTable(
            name: "ChangesLog");

        migrationBuilder.DropTable(
            name: "ChatMessageWorkshops");

        migrationBuilder.DropTable(
            name: "ChildSocialGroup");

        migrationBuilder.DropTable(
            name: "CompanyInformationItems");

        migrationBuilder.DropTable(
            name: "CompetitiveEventDescriptionItems");

        migrationBuilder.DropTable(
            name: "CompetitiveEventRegistrationDeadlines");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents_Contacts_Emails");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents_Contacts_Phones");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents_Contacts_SocialNetworks");

        migrationBuilder.DropTable(
            name: "DataProtectionKeys");

        migrationBuilder.DropTable(
            name: "DateTimeRanges");

        migrationBuilder.DropTable(
            name: "DirectionInstitutionHierarchy");

        migrationBuilder.DropTable(
            name: "ElasticsearchSyncRecords");

        migrationBuilder.DropTable(
            name: "EmployeeChangesLog");

        migrationBuilder.DropTable(
            name: "EmployeeWorkshop");

        migrationBuilder.DropTable(
            name: "Favorites");

        migrationBuilder.DropTable(
            name: "FilesInDb");

        migrationBuilder.DropTable(
            name: "InstitutionAdmins");

        migrationBuilder.DropTable(
            name: "InstitutionFieldDescriptions");

        migrationBuilder.DropTable(
            name: "Judges");

        migrationBuilder.DropTable(
            name: "Notifications");

        migrationBuilder.DropTable(
            name: "Officials");

        migrationBuilder.DropTable(
            name: "OperationsWithObjects");

        migrationBuilder.DropTable(
            name: "ParentBlockedByAdminLog");

        migrationBuilder.DropTable(
            name: "PermissionsForRoles");

        migrationBuilder.DropTable(
            name: "ProviderImages");

        migrationBuilder.DropTable(
            name: "ProviderSectionItems");

        migrationBuilder.DropTable(
            name: "QuartzJobs");

        migrationBuilder.DropTable(
            name: "Ratings");

        migrationBuilder.DropTable(
            name: "RegionAdmins");

        migrationBuilder.DropTable(
            name: "StatisticReports");

        migrationBuilder.DropTable(
            name: "StatisticReportsCSV");

        migrationBuilder.DropTable(
            name: "StudySubjects");

        migrationBuilder.DropTable(
            name: "TagWorkshop");

        migrationBuilder.DropTable(
            name: "TeacherDraft");

        migrationBuilder.DropTable(
            name: "WorkshopDescriptionItems");

        migrationBuilder.DropTable(
            name: "WorkshopDraftImages");

        migrationBuilder.DropTable(
            name: "WorkshopImages");

        migrationBuilder.DropTable(
            name: "Workshops_Contacts_Emails");

        migrationBuilder.DropTable(
            name: "Workshops_Contacts_Phones");

        migrationBuilder.DropTable(
            name: "Workshops_Contacts_SocialNetworks");

        migrationBuilder.DropTable(
            name: "Achievements");

        migrationBuilder.DropTable(
            name: "AspNetRoles");

        migrationBuilder.DropTable(
            name: "ChatRoomWorkshops");

        migrationBuilder.DropTable(
            name: "Children");

        migrationBuilder.DropTable(
            name: "SocialGroups");

        migrationBuilder.DropTable(
            name: "CompanyInformation");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents_Contacts");

        migrationBuilder.DropTable(
            name: "Directions");

        migrationBuilder.DropTable(
            name: "Employees");

        migrationBuilder.DropTable(
            name: "Individuals");

        migrationBuilder.DropTable(
            name: "Positions");

        migrationBuilder.DropTable(
            name: "Languages");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "WorkshopDrafts");

        migrationBuilder.DropTable(
            name: "Workshops_Contacts");

        migrationBuilder.DropTable(
            name: "AchievementTypes");

        migrationBuilder.DropTable(
            name: "Parents");

        migrationBuilder.DropTable(
            name: "Workshops");

        migrationBuilder.DropTable(
            name: "Teachers");

        migrationBuilder.DropTable(
            name: "CATOTTGs");

        migrationBuilder.DropTable(
            name: "AspNetUsers");

        migrationBuilder.DropTable(
            name: "Institutions");

        migrationBuilder.DropTable(
            name: "Providers");

        migrationBuilder.DropTable(
            name: "Addresses");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents");

        migrationBuilder.DropTable(
            name: "InstitutionStatuses");

        migrationBuilder.DropTable(
            name: "ProviderTypes");

        migrationBuilder.DropTable(
            name: "CompetitiveEventAccountingTypes");

        migrationBuilder.DropTable(
            name: "CompetitiveEventCoverages");

        migrationBuilder.DropTable(
            name: "InstitutionHierarchies");
    }
}