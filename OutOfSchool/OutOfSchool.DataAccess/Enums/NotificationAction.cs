using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationAction
{
    Unknown,
    Create,
    Update,
    Delete,
    Message,
    LicenseApproval,
    Block,
    Unblock,
    ProviderBlock,
    ProviderUnblock,
}