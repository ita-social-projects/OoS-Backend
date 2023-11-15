using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.WebApi.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum Feature
{
    Release1,
    Release2,
    Release3,
    Images,
}