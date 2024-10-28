using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OwnershipType
{
    State,
    Common,
    Private,
}