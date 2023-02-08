namespace OutOfSchool.WebApi.Models.Providers;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum OwnershipTypeDto
{
    State,
    Common,
}