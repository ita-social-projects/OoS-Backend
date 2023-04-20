using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class SoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Achievements_AchievementTypes_AchievementTypeId",
            table: "Achievements");

        migrationBuilder.DropForeignKey(
            name: "FK_Applications_Parents_ParentId",
            table: "Applications");

        migrationBuilder.DropForeignKey(
            name: "FK_ProviderAdmins_Providers_ProviderId",
            table: "ProviderAdmins");

        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops");

        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Providers_ProviderId",
            table: "Workshops");

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Workshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "WorkshopDescriptionItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Teachers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "SocialGroups",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Ratings",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "ProviderSectionItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Providers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "ProviderAdmins",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Parents",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionStatuses",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Institutions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionHierarchies",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "InstitutionFieldDescriptions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Favorites",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Directions",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Departments",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "DateTimeRanges",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformationItems",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "CompanyInformation",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Codeficators",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Classes",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Cities",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Children",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "ChatRoomWorkshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "ChatMessageWorkshops",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "BlockedProviderParents",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "AspNetUsers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Applications",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Addresses",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTypes",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "AchievementTeachers",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Achievements",
            type: "tinyint(1)",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddForeignKey(
            name: "FK_Achievements_AchievementTypes_AchievementTypeId",
            table: "Achievements",
            column: "AchievementTypeId",
            principalTable: "AchievementTypes",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Applications_Parents_ParentId",
            table: "Applications",
            column: "ParentId",
            principalTable: "Parents",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_ProviderAdmins_Providers_ProviderId",
            table: "ProviderAdmins",
            column: "ProviderId",
            principalTable: "Providers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops",
            column: "DirectionId",
            principalTable: "Directions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Providers_ProviderId",
            table: "Workshops",
            column: "ProviderId",
            principalTable: "Providers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Achievements_AchievementTypes_AchievementTypeId",
            table: "Achievements");

        migrationBuilder.DropForeignKey(
            name: "FK_Applications_Parents_ParentId",
            table: "Applications");

        migrationBuilder.DropForeignKey(
            name: "FK_ProviderAdmins_Providers_ProviderId",
            table: "ProviderAdmins");

        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops");

        migrationBuilder.DropForeignKey(
            name: "FK_Workshops_Providers_ProviderId",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Workshops");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "WorkshopDescriptionItems");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Teachers");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "SocialGroups");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Ratings");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "ProviderSectionItems");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Providers");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "ProviderAdmins");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Parents");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "InstitutionStatuses");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Institutions");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "InstitutionHierarchies");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "InstitutionFieldDescriptions");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Favorites");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Directions");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Departments");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "DateTimeRanges");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "CompanyInformationItems");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "CompanyInformation");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Codeficators");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Classes");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Cities");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Children");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "ChatRoomWorkshops");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "ChatMessageWorkshops");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "BlockedProviderParents");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Applications");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Addresses");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "AchievementTypes");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "AchievementTeachers");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Achievements");

        migrationBuilder.AddForeignKey(
            name: "FK_Achievements_AchievementTypes_AchievementTypeId",
            table: "Achievements",
            column: "AchievementTypeId",
            principalTable: "AchievementTypes",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Applications_Parents_ParentId",
            table: "Applications",
            column: "ParentId",
            principalTable: "Parents",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_ProviderAdmins_Providers_ProviderId",
            table: "ProviderAdmins",
            column: "ProviderId",
            principalTable: "Providers",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Directions_DirectionId",
            table: "Workshops",
            column: "DirectionId",
            principalTable: "Directions",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_Workshops_Providers_ProviderId",
            table: "Workshops",
            column: "ProviderId",
            principalTable: "Providers",
            principalColumn: "Id");
    }
}
