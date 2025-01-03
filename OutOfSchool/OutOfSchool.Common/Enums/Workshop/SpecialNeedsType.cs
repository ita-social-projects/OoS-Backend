using System.Text.Json.Serialization;

namespace OutOfSchool.Common.Enums.Workshop;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SpecialNeedsType
{
    None,
    Hearing,
    Speaking,
    Sight,
    Intelligence,
    Musculoskeletal,
    Retardation
}