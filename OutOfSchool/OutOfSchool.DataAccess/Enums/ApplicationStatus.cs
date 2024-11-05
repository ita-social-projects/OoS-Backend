using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums;

// TODO: Swagger ignores this attribute on model property in webapi layer
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationStatus
{
    Pending = 1,
    AcceptedForSelection,
    Approved,
    StudyingForYears,
    Completed,
    Rejected,
    Left,
}