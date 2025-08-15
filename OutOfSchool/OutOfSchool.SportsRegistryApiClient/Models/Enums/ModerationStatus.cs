using System.Text.Json.Serialization;

namespace OutOfSchool.SportsRegistryApiClient.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationStatus
{
    active,
    Inactive,
    OnModeration,
    Archived,
    Deleted,
    Draft
}