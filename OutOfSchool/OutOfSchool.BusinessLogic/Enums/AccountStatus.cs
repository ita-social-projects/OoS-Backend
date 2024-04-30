using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum AccountStatus
{
    NeverLogged = 10,
    Accepted = 20,
    Blocked = 30,
}
