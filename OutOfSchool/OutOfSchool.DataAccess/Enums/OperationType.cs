using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OperationType
{
    Create,
    Delete,
    Block,
    Update,
    Reinvite,
}