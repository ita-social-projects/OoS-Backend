using OutOfSchool.QuartzJobs.Api.Logging;
using OutOfSchool.QuartzJobs.Api.Models;
using Quartz;

namespace OutOfSchool.QuartzJobs.Api.Listeners;

public class JobMonitoringListener : IJobListener
{
    private readonly IJobExecutionLogger logger;

    public JobMonitoringListener(IJobExecutionLogger logger)
    {
        this.logger = logger;
    }

    public string Name => nameof(JobMonitoringListener);

    /// <summary>
    /// Called before the job is executed.
    /// Saves the current UTC time to the job context for later duration calculation.
    /// </summary>
    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Save start time to context
        context.MergedJobDataMap.Put("__StartTime", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called if the job execution was vetoed and did not run.
    /// No logging is performed in this case.
    /// </summary>
    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // If the job was blocked — do not log anything
        return Task.CompletedTask;
    }

    // TODO: Investigate adding logic to populate RetryCount and job/trigger parameters inside the jobs themselves.
    // Currently, these values are either null or empty in Redis logs, because jobs do not explicitly handle or expose them.
    // Consider extending job implementations to provide meaningful retry info and execution parameters for monitoring purposes.

    /// <summary>
    /// Called after the job has been executed (successfully or not).
    /// Logs the job execution details to the configured logger.
    /// </summary>
    public Task JobWasExecuted(
    IJobExecutionContext context,
    JobExecutionException? jobException,
    CancellationToken cancellationToken = default)
    {
        var startTime = context.MergedJobDataMap.Get("__StartTime") as DateTime? ?? DateTime.UtcNow;
        var endTime = DateTime.UtcNow;

        var info = new JobExecutionInfo
        {
            JobName = context.JobDetail.Key.Name,
            JobGroup = context.JobDetail.Key.Group,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            StartTime = startTime.ToLocalTime(),
            EndTime = endTime.ToLocalTime(),
            Duration = context.JobRunTime,
            WasSuccessful = jobException is null,
            RetryCount = context.RefireCount,
            TriggerType = context.Trigger.GetType().Name,
            ErrorMessage = jobException?.Message ?? "No error message available",
            StackTrace = jobException?.ToString() ?? "No exception details available",
            Parameters = context.MergedJobDataMap
                .Where(kv => kv.Key != "__StartTime")
                .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "null")
        };

        return logger.LogAsync(info);
    }
}
