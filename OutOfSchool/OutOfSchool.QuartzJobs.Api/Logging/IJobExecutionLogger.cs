using OutOfSchool.QuartzJobs.Api.Models;

namespace OutOfSchool.QuartzJobs.Api.Logging;

public interface IJobExecutionLogger
{
    /// <summary>
    /// Logs last job execution info to the storage.
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    Task LogAsync(JobExecutionInfo info);

    /// <summary>
    /// Returns last job execution info.
    /// </summary>
    Task<IReadOnlyList<JobExecutionInfo>> GetHistoryAsync(string jobName);

    /// <summary>
    /// Returns only failed job executions.
    /// </summary>
    Task<IReadOnlyList<JobExecutionInfo>> GetFailedAsync(string jobName);
}
