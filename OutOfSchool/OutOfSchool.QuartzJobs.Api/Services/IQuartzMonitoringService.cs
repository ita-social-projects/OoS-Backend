using OutOfSchool.QuartzJobs.Api.Models;

namespace OutOfSchool.QuartzJobs.Api.Services;

/// Provides operations for retrieving monitoring information about Quartz jobs.
public interface IQuartzMonitoringService
{
    /// <summary>
    /// Retrieves information about all registered jobs with their current status.
    /// </summary>
    /// <returns>A read-only list of job summaries.</returns>
    Task<IReadOnlyList<JobInfoDto>> GetAllJobsAsync();

    /// <summary>
    /// Retrieves information about currently running jobs.
    /// </summary>
    /// <returns>A read-only list of running jobs with execution details.</returns>
    Task<IReadOnlyList<RunningJobDto>> GetRunningJobsAsync();

    /// <summary>
    /// Returns job details, trigger keys, and upcoming execution times for a specific job by name.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>Detailed job information if found; otherwise, null.</returns>
    Task<JobDetailsDto?> GetJobDetailsAsync(string jobName);

    /// <summary>
    /// Retrieves execution history (success and failed executions) of a specific job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>A read-only list of job execution records.</returns>
    Task<IReadOnlyList<JobExecutionInfo>> GetJobHistoryAsync(string jobName);

    /// <summary>
    /// Retrieves only failed execution records of a specific job.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>A read-only list of failed job execution records.</returns>
    Task<IReadOnlyList<JobExecutionInfo>> GetFailedJobsAsync(string jobName);

    /// <summary>
    /// Retrieves the next planned execution times for all jobs and their triggers.
    /// </summary>
    /// <returns>A read-only list of upcoming executions for all jobs.</returns>
    Task<IReadOnlyList<JobNextExecutionDto>> GetNextExecutionsForAllJobsAsync();

    /// <summary>
    /// Retrieves the next planned execution times for a specific job and its triggers.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <returns>A read-only list of upcoming executions for the specified job.</returns>
    Task<IReadOnlyList<JobNextExecutionDto>> GetNextExecutionsForJobAsync(string jobName);
}