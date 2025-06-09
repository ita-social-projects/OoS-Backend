namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents the upcoming executions for a specific job and its triggers.
/// </summary>
public record JobNextExecutionDto
{
    public string JobName { get; set; } = default!;
    public string JobGroup { get; set; } = default!;
    public List<TriggerExecutionDto> NextExecutions { get; set; } = new();
}
