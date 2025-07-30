using System.Text.Json.Serialization;

namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SectionScheduleRequest
{
    [JsonPropertyName("sectionScheduleWeekday")]
    public string SectionScheduleWeekday { get; set; } = null!;

    [JsonPropertyName("sectionScheduleTimeFrom")]
    public string SectionScheduleTimeFrom { get; set; } = null!; // e.g., "09:00:00"

    [JsonPropertyName("sectionScheduleTimeTo")]
    public string SectionScheduleTimeTo { get; set; } = null!;
}