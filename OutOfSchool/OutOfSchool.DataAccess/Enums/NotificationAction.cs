using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum NotificationAction
{
    Unknown,
    Create,
    Update,
    Delete,
    Message,
    LicenseApproval,
}