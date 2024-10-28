using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShowApplications
{
    All,
    Blocked,
    Unblocked,
}
