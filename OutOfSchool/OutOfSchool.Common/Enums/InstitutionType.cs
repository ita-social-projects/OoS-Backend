using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InstitutionType
    {
        Comprehensive,
        Prof,
        Specialized,
    }
}