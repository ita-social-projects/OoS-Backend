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

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Save start time to context
        context.MergedJobDataMap.Put("__StartTime", DateTime.UtcNow);
        return Task.CompletedTask;
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // If the job was blocked — do not log anything
        return Task.CompletedTask;
    }

    public Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        var startTime = context.MergedJobDataMap.Get("__StartTime") as DateTime? ?? DateTime.UtcNow;
        var endTime = DateTime.UtcNow;

        var triggerType = context.Trigger.GetType().Name;
        var cronTrigger = context.Trigger as ICronTrigger;
        var simpleTrigger = context.Trigger as ISimpleTrigger;

        var info = new JobExecutionInfo
        {
            JobName = context.JobDetail.Key.Name,
            JobGroup = context.JobDetail.Key.Group,
            TriggerName = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group,
            StartTime = startTime,
            EndTime = endTime,
            Duration = context.JobRunTime,
            WasSuccessful = jobException == null,
            RetryCount = context.RefireCount,
            TriggerType = triggerType,
            CronExpression = cronTrigger?.CronExpressionString,
            RepeatInterval = simpleTrigger?.RepeatInterval.ToString(),
            ErrorMessage = jobException?.Message,
            StackTrace = jobException?.InnerException?.ToString(),
            Parameters = context.MergedJobDataMap
        .Where(kv => kv.Key != "__StartTime")
        .ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? "null")
        };

        return logger.LogAsync(info);
    }
}
