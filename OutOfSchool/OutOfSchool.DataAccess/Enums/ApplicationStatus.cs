using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
    // TODO: Swagger ignores this attribute on model property in webapi layer
    [JsonConverter(typeof(StringEnumConverter))]
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
}
