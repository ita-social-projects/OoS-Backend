using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OutOfSchool.Services.Enums
{
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
