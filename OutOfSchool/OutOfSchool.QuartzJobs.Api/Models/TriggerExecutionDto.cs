namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents the upcoming execution times for a specific trigger.
/// </summary>
public record TriggerExecutionDto
{
    public string TriggerName { get; set; } = default!;
    public string TriggerGroup { get; set; } = default!;
    public List<DateTimeOffset> NextExecutions { get; set; } = new();
}