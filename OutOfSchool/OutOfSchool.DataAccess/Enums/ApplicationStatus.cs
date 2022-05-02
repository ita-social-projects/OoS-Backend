using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
    // TODO: Swagger ignores this attribute on model property in webapi layer
    // Notice that statuses less or equal StudyingForYears are used to block ability to create a new application in AllowedNewApplicationByChildStatus method
    // so be careful if you wanna change this enum
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
