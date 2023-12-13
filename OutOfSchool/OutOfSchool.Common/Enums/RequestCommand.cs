using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum RequestCommand
{
    Create,
    Update,
    Delete,
    Block,
    Reinvite,
}
