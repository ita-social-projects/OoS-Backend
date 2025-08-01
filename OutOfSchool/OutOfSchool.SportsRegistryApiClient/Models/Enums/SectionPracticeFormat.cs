using System.Text.Json.Serialization;
namespace OutOfSchool.SportsRegistryApiClient.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SectionPracticeFormat
{
    ONLINE,
    OFFLINE,
    HYBRID
}