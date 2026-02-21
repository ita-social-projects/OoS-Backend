using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums.CompetitiveEventStatus;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CompetitiveEventDraftStatus
{
    Draft = 1,
    PendingModeration,
    Rejected,
    EditedByModerator
}
