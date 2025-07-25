namespace OutOfSchool.SportsRegistryApiClient.Models.Requests;

public class SectionScheduleRequest
{
    public string SectionScheduleWeekday { get; set; } = null!; // e.g. MONDAY
    public string SectionScheduleTimeFrom { get; set; } = null!; // e.g. 09:00:00
    public string SectionScheduleTimeTo { get; set; } = null!;
}