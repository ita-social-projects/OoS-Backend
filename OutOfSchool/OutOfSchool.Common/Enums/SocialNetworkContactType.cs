using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SocialNetworkContactType
{
    Instagram = 0,
    Facebook = 1,
    Website = 2,
}
