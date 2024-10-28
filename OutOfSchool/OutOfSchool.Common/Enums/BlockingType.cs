using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BlockingType
{
    None,
    Manually,
    Automatically,
}
