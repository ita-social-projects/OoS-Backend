using System.Text.Json.Serialization;

namespace OutOfSchool.SportsRegistryApiClient.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ModerationStatus
{
    ACTIVE,
    INACTIVE,
    ONMODERATION,
    ARCHIVED,
    DELETED,
    DRAFT
}