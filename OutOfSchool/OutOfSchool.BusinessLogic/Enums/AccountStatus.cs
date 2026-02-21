using System.Text.Json.Serialization;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountStatus
{
    NeverLogged = 10,
    Accepted = 20,
    Blocked = 30,
}
