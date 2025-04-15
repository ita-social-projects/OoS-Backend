namespace OutOfSchool.QuartzJobs.Api.Models;

/// <summary>
/// Represents information about a job execution, including runtime, result, and parameters.
/// </summary>
public class JobExecutionInfo
{
    public string JobName { get; set; } = string.Empty;
    public string? JobGroup { get; set; }
    public string? TriggerName { get; set; }
    public string? TriggerGroup { get; set; }

    public int RetryCount { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration { get; set; }

    public bool WasSuccessful { get; set; }

    public string? TriggerType { get; set; }

    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
}
