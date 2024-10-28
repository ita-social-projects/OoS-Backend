using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    Application,
    Chat,
    Workshop,
    Provider,
    System,
    Parent, //LicenseApproval
}