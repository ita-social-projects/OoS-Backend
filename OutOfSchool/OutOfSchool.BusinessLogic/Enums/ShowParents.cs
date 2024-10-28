using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShowParents
{
    All,
    Blocked,
    Unblocked,
}
