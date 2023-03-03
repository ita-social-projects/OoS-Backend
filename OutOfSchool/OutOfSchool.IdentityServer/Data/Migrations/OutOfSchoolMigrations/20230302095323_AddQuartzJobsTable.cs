using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddQuartzJobsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreationTime",
            table: "AverageRatings");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Workshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "WorkshopDescriptionItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Teachers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "SocialGroups",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "RegionAdmins",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Ratings",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ProviderSectionItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ProviderAdmins",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Parents",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionStatuses",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Institutions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionHierarchies",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionFieldDescriptions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Favorites",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Directions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "DateTimeRanges",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformationItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformation",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Children",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ChatRoomWorkshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ChatMessageWorkshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CATOTTGs",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "BlockedProviderParents",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AspNetUsers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Applications",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Addresses",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTypes",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTeachers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Achievements",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)");

        migrationBuilder.CreateTable(
            name: "QuartzJobs",
            columns: table => new
            {
                Id = table.Column<long>(type: "bigint", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Name = table.Column<string>(type: "longtext", nullable: true)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                LastSuccessLaunch = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: false),
                IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_QuartzJobs", x => x.Id);
            })
            .Annotation("MySql:CharSet", "utf8mb4");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "QuartzJobs");

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Workshops",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "WorkshopDescriptionItems",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Teachers",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "SocialGroups",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "RegionAdmins",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Ratings",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ProviderSectionItems",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ProviderAdmins",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Parents",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionStatuses",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Institutions",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionHierarchies",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionFieldDescriptions",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Favorites",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Directions",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "DateTimeRanges",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformationItems",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformation",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Children",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ChatRoomWorkshops",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "ChatMessageWorkshops",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "CATOTTGs",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "BlockedProviderParents",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "CreationTime",
            table: "AverageRatings",
            type: "datetime(6)",
            nullable: false,
            defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AspNetUsers",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Applications",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Addresses",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTypes",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTeachers",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);

        migrationBuilder.AlterColumn<bool>(
            name: "IsDeleted",
            table: "Achievements",
            type: "tinyint(1)",
            nullable: false,
            oldClrType: typeof(bool),
            oldType: "tinyint(1)",
            oldDefaultValue: false);
    }
}
