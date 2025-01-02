using System.Text.Json.Serialization;

namespace OutOfSchool.Services.Enums.WorkshopStatus;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkshopDraftStatus
{
    Draft = 1,
    PendingModeration,
    Rejected
}
