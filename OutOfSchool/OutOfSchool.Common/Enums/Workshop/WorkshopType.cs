using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums.Workshop;

// TODO: Add new types when customer specifies them
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkshopType
{
    None
}