using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OutOfSchool.IdentityServer.Data.Migrations.OutOfSchoolMigrations;

public partial class AddIndexesToSoftDelete : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Workshops_IsDeleted",
            table: "Workshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_WorkshopDescriptionItems_IsDeleted",
            table: "WorkshopDescriptionItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Teachers_IsDeleted",
            table: "Teachers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_SocialGroups_IsDeleted",
            table: "SocialGroups",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_RegionAdmins_IsDeleted",
            table: "RegionAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Ratings_IsDeleted",
            table: "Ratings",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProviderSectionItems_IsDeleted",
            table: "ProviderSectionItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Providers_IsDeleted",
            table: "Providers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ProviderAdmins_IsDeleted",
            table: "ProviderAdmins",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Parents_IsDeleted",
            table: "Parents",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionStatuses_IsDeleted",
            table: "InstitutionStatuses",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Institutions_IsDeleted",
            table: "Institutions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionHierarchies_IsDeleted",
            table: "InstitutionHierarchies",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_InstitutionFieldDescriptions_IsDeleted",
            table: "InstitutionFieldDescriptions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Favorites_IsDeleted",
            table: "Favorites",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Directions_IsDeleted",
            table: "Directions",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_DateTimeRanges_IsDeleted",
            table: "DateTimeRanges",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformationItems_IsDeleted",
            table: "CompanyInformationItems",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CompanyInformation_IsDeleted",
            table: "CompanyInformation",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Children_IsDeleted",
            table: "Children",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ChatRoomWorkshops_IsDeleted",
            table: "ChatRoomWorkshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_ChatMessageWorkshops_IsDeleted",
            table: "ChatMessageWorkshops",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_CATOTTGs_IsDeleted",
            table: "CATOTTGs",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_BlockedProviderParents_IsDeleted",
            table: "BlockedProviderParents",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AspNetUsers_IsDeleted",
            table: "AspNetUsers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Applications_IsDeleted",
            table: "Applications",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Addresses_IsDeleted",
            table: "Addresses",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTypes_IsDeleted",
            table: "AchievementTypes",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementTeachers_IsDeleted",
            table: "AchievementTeachers",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_IsDeleted",
            table: "Achievements",
            column: "IsDeleted");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Workshops_IsDeleted",
            table: "Workshops");

        migrationBuilder.DropIndex(
            name: "IX_WorkshopDescriptionItems_IsDeleted",
            table: "WorkshopDescriptionItems");

        migrationBuilder.DropIndex(
            name: "IX_Teachers_IsDeleted",
            table: "Teachers");

        migrationBuilder.DropIndex(
            name: "IX_SocialGroups_IsDeleted",
            table: "SocialGroups");

        migrationBuilder.DropIndex(
            name: "IX_RegionAdmins_IsDeleted",
            table: "RegionAdmins");

        migrationBuilder.DropIndex(
            name: "IX_Ratings_IsDeleted",
            table: "Ratings");

        migrationBuilder.DropIndex(
            name: "IX_ProviderSectionItems_IsDeleted",
            table: "ProviderSectionItems");

        migrationBuilder.DropIndex(
            name: "IX_Providers_IsDeleted",
            table: "Providers");

        migrationBuilder.DropIndex(
            name: "IX_ProviderAdmins_IsDeleted",
            table: "ProviderAdmins");

        migrationBuilder.DropIndex(
            name: "IX_Parents_IsDeleted",
            table: "Parents");

        migrationBuilder.DropIndex(
            name: "IX_InstitutionStatuses_IsDeleted",
            table: "InstitutionStatuses");

        migrationBuilder.DropIndex(
            name: "IX_Institutions_IsDeleted",
            table: "Institutions");

        migrationBuilder.DropIndex(
            name: "IX_InstitutionHierarchies_IsDeleted",
            table: "InstitutionHierarchies");

        migrationBuilder.DropIndex(
            name: "IX_InstitutionFieldDescriptions_IsDeleted",
            table: "InstitutionFieldDescriptions");

        migrationBuilder.DropIndex(
            name: "IX_Favorites_IsDeleted",
            table: "Favorites");

        migrationBuilder.DropIndex(
            name: "IX_Directions_IsDeleted",
            table: "Directions");

        migrationBuilder.DropIndex(
            name: "IX_DateTimeRanges_IsDeleted",
            table: "DateTimeRanges");

        migrationBuilder.DropIndex(
            name: "IX_CompanyInformationItems_IsDeleted",
            table: "CompanyInformationItems");

        migrationBuilder.DropIndex(
            name: "IX_CompanyInformation_IsDeleted",
            table: "CompanyInformation");

        migrationBuilder.DropIndex(
            name: "IX_Children_IsDeleted",
            table: "Children");

        migrationBuilder.DropIndex(
            name: "IX_ChatRoomWorkshops_IsDeleted",
            table: "ChatRoomWorkshops");

        migrationBuilder.DropIndex(
            name: "IX_ChatMessageWorkshops_IsDeleted",
            table: "ChatMessageWorkshops");

        migrationBuilder.DropIndex(
            name: "IX_CATOTTGs_IsDeleted",
            table: "CATOTTGs");

        migrationBuilder.DropIndex(
            name: "IX_BlockedProviderParents_IsDeleted",
            table: "BlockedProviderParents");

        migrationBuilder.DropIndex(
            name: "IX_AspNetUsers_IsDeleted",
            table: "AspNetUsers");

        migrationBuilder.DropIndex(
            name: "IX_Applications_IsDeleted",
            table: "Applications");

        migrationBuilder.DropIndex(
            name: "IX_Addresses_IsDeleted",
            table: "Addresses");

        migrationBuilder.DropIndex(
            name: "IX_AchievementTypes_IsDeleted",
            table: "AchievementTypes");

        migrationBuilder.DropIndex(
            name: "IX_AchievementTeachers_IsDeleted",
            table: "AchievementTeachers");

        migrationBuilder.DropIndex(
            name: "IX_Achievements_IsDeleted",
            table: "Achievements");
    }
}
