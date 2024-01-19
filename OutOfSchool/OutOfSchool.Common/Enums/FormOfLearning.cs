using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum FormOfLearning
{
    Offline = 10,
    Online = 20,
    Mixed = 30,
}
