using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PayRateType
    {
        None = 0,
        Classes = 1,
        Month,
        Day,
        Year,
        Hour,
        Course,
        AllPeriod,
    }
}
