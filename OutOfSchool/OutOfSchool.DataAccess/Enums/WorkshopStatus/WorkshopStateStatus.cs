using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums.WorkshopStatus;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkshopStateStatus
{
    Draft = 1,
    PendingModeration,
    Editing,
    Active,
    Inactive,
    Archived,
    Deleted,
}
