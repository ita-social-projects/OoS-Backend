using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums.Workshop;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AgeComposition
{
    // одновікова
    SameAge,
    // різновікова
    DifferentAge,
}