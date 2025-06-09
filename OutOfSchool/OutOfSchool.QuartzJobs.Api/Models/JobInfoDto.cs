namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents summarized information about a registered Quartz job.
/// </summary>
public record JobInfoDto
{
    public string Name { get; init; } = default!;
    public string Group { get; init; } = default!;
    public string JobType { get; init; } = default!;
    public string Status { get; init; } = default!;
    public string? Description { get; init; }
}
