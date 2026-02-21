using System.Text.Json.Serialization;
namespace OutOfSchool.SportsRegistryApiClient.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Weekday
{
    MONDAY,
    TUESDAY,
    WEDNESDAY,
    THURSDAY,
    FRIDAY,
    SATURDAY,
    SUNDAY
}
