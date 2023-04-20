using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum WorkshopStatus
{
    Open = 1,
    Closed,
}