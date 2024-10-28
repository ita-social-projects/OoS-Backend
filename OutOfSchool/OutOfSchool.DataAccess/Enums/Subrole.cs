using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Subrole
{
    None,
    ProviderDeputy,
    ProviderAdmin,
}