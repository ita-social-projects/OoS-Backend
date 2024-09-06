using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OutOfSchool.Migrations.Data.Migrations.OutOfSchoolMigrations;

/// <inheritdoc />
public partial class CompetitiveEvents : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "CompetitiveEventId",
            table: "Providers",
            type: "binary(16)",
            nullable: true);

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
            name: "CompetitiveEvents",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                Title = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ShortTitle = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                State = table.Column<int>(type: "int", nullable: false),
                RegistrationStartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                RegistrationEndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                ParentId = table.Column<Guid>(type: "binary(16)", nullable: false),
                BuildingHoldingId = table.Column<Guid>(type: "binary(16)", nullable: false),
                ChildParticipantId = table.Column<Guid>(type: "binary(16)", nullable: false),
                ChiefJudgeId = table.Column<Guid>(type: "binary(16)", nullable: false),
                AdditionalDescription = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                ScheduledStartTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                ScheduledEndTime = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                NumberOfSeats = table.Column<uint>(type: "int unsigned", nullable: false),
                Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                DescriptionOfTheEnrollmentProcedure = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                OrganizerOfTheEventId = table.Column<Guid>(type: "binary(16)", nullable: false),
                PlannedFormatOfClasses = table.Column<int>(type: "int", nullable: false),
                VenueId = table.Column<Guid>(type: "binary(16)", nullable: false),
                PreferentialTermsOfParticipation = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                AreThereBenefits = table.Column<bool>(type: "tinyint(1)", nullable: false),
                Rating = table.Column<uint>(type: "int unsigned", nullable: false),
                NumberOfRatings = table.Column<uint>(type: "int unsigned", nullable: false),
                OptionsForPeopleWithDisabilities = table.Column<bool>(type: "tinyint(1)", nullable: false),
                DescriptionOfOptionsForPeopleWithDisabilities = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CategoryId = table.Column<long>(type: "bigint", nullable: false),
                Subcategory = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MinimumAge = table.Column<int>(type: "int", nullable: false),
                MaximumAge = table.Column<int>(type: "int", nullable: false),
                Price = table.Column<int>(type: "int", nullable: false),
                CompetitiveSelection = table.Column<bool>(type: "tinyint(1)", nullable: false),
                NumberOfOccupiedSeats = table.Column<uint>(type: "int unsigned", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompetitiveEvents", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompetitiveEvents_CompetitiveEvents_ParentId",
                    column: x => x.ParentId,
                    principalTable: "CompetitiveEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CompetitiveEvents_Directions_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Directions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CompetitiveEvents_Providers_OrganizerOfTheEventId",
                    column: x => x.OrganizerOfTheEventId,
                    principalTable: "Providers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
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
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CompetitiveEventId = table.Column<Guid>(type: "binary(16)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompetitiveEventAccountingTypes", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompetitiveEventAccountingTypes_CompetitiveEvents_Competitiv~",
                    column: x => x.CompetitiveEventId,
                    principalTable: "CompetitiveEvents",
                    principalColumn: "Id");
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
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CompetitiveEventId = table.Column<Guid>(type: "binary(16)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompetitiveEventCoverages", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompetitiveEventCoverages_CompetitiveEvents_CompetitiveEvent~",
                    column: x => x.CompetitiveEventId,
                    principalTable: "CompetitiveEvents",
                    principalColumn: "Id");
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateTable(
            name: "CompetitiveEventDescriptionItems",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "binary(16)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                SectionName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                CompetitiveEventId = table.Column<Guid>(type: "binary(16)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CompetitiveEventDescriptionItems", x => x.Id);
                table.ForeignKey(
                    name: "FK_CompetitiveEventDescriptionItems_CompetitiveEvents_Competiti~",
                    column: x => x.CompetitiveEventId,
                    principalTable: "CompetitiveEvents",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.InsertData(
            table: "CompetitiveEventAccountingTypes",
            columns: new[] { "Id", "CompetitiveEventId", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, null, "Освітній проєкт", "Educational project" },
                { 2, null, "Конкурс (не має етапів)", "Competition" },
                { 3, null, "Основний конкурс (має мати підпорядковані конкурси-етапи)", "Main competition" },
                { 4, null, "Етап конкурсу (має мати батьківський основний конкурс)", "Contest stage" }
            });

        migrationBuilder.InsertData(
            table: "CompetitiveEventCoverages",
            columns: new[] { "Id", "CompetitiveEventId", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, null, "Локальний (Шкільний)", "Local (School)" },
                { 2, null, "Міський", "City" },
                { 3, null, "Районний", "Raional" },
                { 4, null, "Обласний", "Regional" },
                { 5, null, "Всеукраїнський", "All-Ukrainian" },
                { 6, null, "Міжнародний", "International" }
            });

        migrationBuilder.InsertData(
            table: "CompetitiveEventRegistrationDeadlines",
            columns: new[] { "Id", "Title", "TitleEn" },
            values: new object[,]
            {
                { 1, "Постійно (протягом року)", "Constantly (during the year)" },
                { 2, "Певний місяць або місяці року", "A certain month or months of the year" }
            });

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11ccnBxb258ent5eH2Qjo+NjJFUZ5Y=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 2L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVJaYl5k=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 3L,
            column: "PackedPermissions",
            value: "ZQMKCwwUFhUXHiAfISgpKz49PFBRVJY=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzI2SEdJRlBRW1xUlg==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoLBRQUUZUblt6eHl7fY6MjY+RZ5Y=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW2eW");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlSMjVtnlg==");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_CompetitiveEventId",
            table: "Providers",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventAccountingTypes_CompetitiveEventId",
            table: "CompetitiveEventAccountingTypes",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventAccountingTypes_IsDeleted",
            table: "CompetitiveEventAccountingTypes",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventCoverages_CompetitiveEventId",
            table: "CompetitiveEventCoverages",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventCoverages_IsDeleted",
            table: "CompetitiveEventCoverages",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventDescriptionItems_CompetitiveEventId",
            table: "CompetitiveEventDescriptionItems",
            column: "CompetitiveEventId");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventDescriptionItems_IsDeleted",
            table: "CompetitiveEventDescriptionItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEventRegistrationDeadlines_IsDeleted",
            table: "CompetitiveEventRegistrationDeadlines",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompetitiveEvents_CategoryId",
            table: "CompetitiveEvents",
            column: "CategoryId");

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

        migrationBuilder.AddForeignKey(
            name: "FK_Providers_CompetitiveEvents_CompetitiveEventId",
            table: "Providers",
            column: "CompetitiveEventId",
            principalTable: "CompetitiveEvents",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Providers_CompetitiveEvents_CompetitiveEventId",
            table: "Providers");

        migrationBuilder.DropTable(
            name: "CompetitiveEventAccountingTypes");

        migrationBuilder.DropTable(
            name: "CompetitiveEventCoverages");

        migrationBuilder.DropTable(
            name: "CompetitiveEventDescriptionItems");

        migrationBuilder.DropTable(
            name: "CompetitiveEventRegistrationDeadlines");

        migrationBuilder.DropTable(
            name: "CompetitiveEvents");

        migrationBuilder.DropIndex(
            name: "IX_Providers_CompetitiveEventId",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "CompetitiveEventId",
            table: "Providers");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 1L,
            column: "PackedPermissions",
            value: "ZGVmAwIBBAoLDQweIB8hKCkrLBc0MzI1Nzg+PTw/SEdJRlBRW11ccnBxb258ent5eH2Qjo+NjJFUZw==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 2L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzQzMjU2SEdJRlBRW11cVA==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 3L,
            column: "PackedPermissions",
            value: "ZQMKCwwUFhUXHiAfISgpKz49PFBRVA==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 4L,
            column: "PackedPermissions",
            value: "ZQMCAQQKCzI2SEdJRlBRW1xU");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 5L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoLBRQUUZUblt6eHl7fY6MjY+RZw==");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 6L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlR4eY6MjY+RW2c=");

        migrationBuilder.UpdateData(
            table: "PermissionsForRoles",
            keyColumn: "Id",
            keyValue: 7L,
            column: "PackedPermissions",
            value: "ZWYDAgEECjI1NzgoFFBRRlSMjVtn");
    }
}
