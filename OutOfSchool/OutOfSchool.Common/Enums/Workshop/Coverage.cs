using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums.Workshop;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Coverage
{
    School,
    City,
    District,
    Region,
    AllUkraine,
    International
}