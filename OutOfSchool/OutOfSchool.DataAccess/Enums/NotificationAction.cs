using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationAction
    {
        Unknown,
        Create,
        UpdateApplicationAcceptedForSelection,
        UpdateApplicationApproved,
        UpdateApplicationCompleted,
        UpdateApplicationRejected,
        UpdateApplicationLeft,
        Delete,
        Message,
    }
}
