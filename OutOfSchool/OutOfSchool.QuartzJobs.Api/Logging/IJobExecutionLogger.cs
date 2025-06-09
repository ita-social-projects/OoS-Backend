using OutOfSchool.QuartzJobs.Api.Models;

namespace OutOfSchool.QuartzJobs.Api.Logging;

/// <summary>
/// Provides methods for logging and retrieving job execution information.
/// </summary>
public interface IJobExecutionLogger
{
    /// <summary>
    /// Logs the latest job execution information to the storage.
    /// </summary>
    /// <param name="info">The job execution information to log.</param>
    /// <returns>A completed task.</returns>
    Task LogAsync(JobExecutionInfo info);

    /// <summary>
    /// Retrieves the latest execution history for the specified job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>A list of job execution records, including both successful and failed executions.</returns>
    Task<IReadOnlyList<JobExecutionInfo>> GetHistoryAsync(string jobName);

    /// <summary>
    /// Retrieves only failed execution records for the specified job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>A list of failed job execution records.</returns>
    Task<IReadOnlyList<JobExecutionInfo>> GetFailedAsync(string jobName);
}
