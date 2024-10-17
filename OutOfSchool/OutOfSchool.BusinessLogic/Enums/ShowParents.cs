using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.BusinessLogic.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ShowParents
{
    All,
    Blocked,
    Unblocked,
}
