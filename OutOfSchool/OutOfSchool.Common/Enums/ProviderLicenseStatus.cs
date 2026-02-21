using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProviderLicenseStatus
{
    NotProvided,
    Pending,
    Approved,
}