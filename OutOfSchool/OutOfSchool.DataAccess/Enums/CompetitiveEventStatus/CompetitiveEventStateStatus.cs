using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums.CompetitiveEventStatus;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CompetitiveEventStateStatus
{
    Draft = 1,
    PendingModeration,
    Active,
    Inactive,
    Deleted,
    Published,
    Completed,
    Archived,
}
