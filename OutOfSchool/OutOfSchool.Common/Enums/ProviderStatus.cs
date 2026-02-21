using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProviderStatus
{
    Pending = 10,
    Editing = 20,
    Approved = 40,
    Recheck = 30,
}