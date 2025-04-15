namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents detailed information about a specific Quartz job,
/// including its triggers and status.
/// </summary>
public record JobDetailsDto
{
    public string Name { get; set; } = default!;
    public string Group { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? Description { get; set; }
    public List<JobTriggerDto> Triggers { get; set; } = new();
}