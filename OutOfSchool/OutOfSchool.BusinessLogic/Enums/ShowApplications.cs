using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ShowApplications
{
    All,
    Blocked,
    Unblocked,
}
