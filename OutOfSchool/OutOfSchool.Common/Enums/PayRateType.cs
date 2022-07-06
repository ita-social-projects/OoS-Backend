using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum PayRateType
{
    Classes = 1,
    Month,
    Day,
    Year,
    Hour,
    Course,
    AllPeriod,
}