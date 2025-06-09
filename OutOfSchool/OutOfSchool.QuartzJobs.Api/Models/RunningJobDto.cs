namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents a currently executing Quartz job instance.
/// </summary>
public record RunningJobDto
{
    public string JobName { get; init; } = default!;
    public string Group { get; init; } = default!;
    public DateTimeOffset? StartedAt { get; init; }
    public string Trigger { get; init; } = default!;
    public string TriggerGroup { get; init; } = default!;
}
