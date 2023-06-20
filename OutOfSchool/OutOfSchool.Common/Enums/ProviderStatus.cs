using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ProviderStatus
{
    Pending = 10,
    Editing = 20,
    Approved = 40,
    Recheck = 30,
}