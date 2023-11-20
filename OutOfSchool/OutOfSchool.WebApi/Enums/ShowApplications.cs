using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ShowApplications
{
    All,
    Blocked,
    Unblocked,
}
