using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PayRateType
{
    None,
    Classes,
    Month,
    Day,
    Year,
    Hour,
    Course,
    AllPeriod,
}