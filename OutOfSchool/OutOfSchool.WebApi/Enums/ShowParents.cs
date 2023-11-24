using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ShowParents
{
    All,
    Blocked,
    Unblocked,
}
