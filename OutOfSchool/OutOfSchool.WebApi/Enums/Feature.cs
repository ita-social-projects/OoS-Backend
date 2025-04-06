using System.Text.Json.Serialization;

namespace OutOfSchool.WebApi.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Feature
{
    Release1,
    Release2,
    Release3,
    Images,
    ShowForProduction,
    TechAdminImport,
    TechAdminExport,
    EnableWorkshopGroupTypeField,
    DirectionManagement,
    AchievementManagement,
    AdminsChildrenParentsManagement,
    MessagingFeature,
    PasswordLogin,
    PasswordRegistration,
    EmailConfirmation,
    EmailManagement,
    PasswordManagement,
    OnlyUkrainianLanguage
}