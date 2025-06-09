namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Describes a trigger assigned to a job, including its upcoming execution times.
/// </summary>
public record JobTriggerDto
{
    public string TriggerKey { get; set; } = default!;
    public List<DateTimeOffset> NextExecutions { get; set; } = new();
}