using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SocialNetworkContactType
{
    Instagram,
    Facebook,
    Website,
}
