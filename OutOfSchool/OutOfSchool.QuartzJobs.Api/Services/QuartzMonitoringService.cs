using OutOfSchool.QuartzJobs.Api.Logging;
using Quartz.Impl.Matchers;
using Quartz;
using OutOfSchool.QuartzJobs.Api.Models;
using OutOfSchool.QuartzJobs.Api.Util;

namespace OutOfSchool.QuartzJobs.Api.Services;

/// <inheritdoc />
public class QuartzMonitoringService : IQuartzMonitoringService
{
    private readonly ISchedulerFactory schedulerFactory;
    private readonly IJobExecutionLogger logger;
    private const int DefaultExecutionCount = 5;

    public QuartzMonitoringService(ISchedulerFactory schedulerFactory, IJobExecutionLogger logger)
    {
        this.schedulerFactory = schedulerFactory;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobInfoDto>> GetAllJobsAsync()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());

        var jobs = new List<JobInfoDto>();

        foreach (var jobKey in jobKeys)
        {
            var detail = await scheduler.GetJobDetail(jobKey);
            var status = await GetJobStatus(scheduler, jobKey);

            jobs.Add(new JobInfoDto
            {
                Name = jobKey.Name,
                Group = jobKey.Group,
                JobType = detail.JobType.FullName,
                Status = status.ToString(),
                Description = detail.Description
            });
        }

        return jobs;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RunningJobDto>> GetRunningJobsAsync()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var executing = await scheduler.GetCurrentlyExecutingJobs();

        return executing.Select(context => new RunningJobDto
        {
            JobName = context.JobDetail.Key.Name,
            Group = context.JobDetail.Key.Group,
            StartedAt = context.FireTimeUtc.ToLocalTime(),
            Trigger = context.Trigger.Key.Name,
            TriggerGroup = context.Trigger.Key.Group
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<JobDetailsDto?> GetJobDetailsAsync(string jobName)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobKey = jobKeys.FirstOrDefault(k => k.Name == jobName);
        if (jobKey is null)
            return null;

        var detail = await scheduler.GetJobDetail(jobKey);
        var status = await GetJobStatus(scheduler, jobKey);
        var triggers = await scheduler.GetTriggersOfJob(jobKey);

        return new JobDetailsDto
        {
            Name = jobKey.Name,
            Group = jobKey.Group,
            Description = detail.Description,
            JobType = detail.JobType.FullName,
            Status = status.ToString(),
            Triggers = triggers.Select(t => new JobTriggerDto
            {
                TriggerKey = t.Key.ToString(),
                NextExecutions = t is ICronTrigger cronTrigger
                ? CronHelper.GetUpcomingExecutions(cronTrigger.CronExpressionString, DefaultExecutionCount)
                : new List<DateTimeOffset> { t.GetNextFireTimeUtc()?.ToLocalTime() ?? DateTimeOffset.MinValue }
            }).ToList()
        };
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<JobExecutionInfo>> GetJobHistoryAsync(string jobName) =>
        logger.GetHistoryAsync(jobName);

    /// <inheritdoc />
    public Task<IReadOnlyList<JobExecutionInfo>> GetFailedJobsAsync(string jobName) =>
        logger.GetFailedAsync(jobName);

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobNextExecutionDto>> GetNextExecutionsForAllJobsAsync()
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var result = new List<JobNextExecutionDto>();

        foreach (var jobKey in jobKeys)
        {
            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            result.Add(new JobNextExecutionDto
            {
                JobName = jobKey.Name,
                JobGroup = jobKey.Group,
                NextExecutions = triggers.Select(t => new TriggerExecutionDto
                {
                    TriggerName = t.Key.Name,
                    TriggerGroup = t.Key.Group,
                    NextExecutions = t is ICronTrigger cronTrigger
                    ? CronHelper.GetUpcomingExecutions(cronTrigger.CronExpressionString, DefaultExecutionCount)
                    : new List<DateTimeOffset> { t.GetNextFireTimeUtc()?.ToLocalTime() ?? DateTimeOffset.MinValue }
                }).ToList()
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JobNextExecutionDto>> GetNextExecutionsForJobAsync(string jobName)
    {
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var jobKey = jobKeys.FirstOrDefault(k => k.Name == jobName);
        if (jobKey is null)
            return new List<JobNextExecutionDto>();

        var triggers = await scheduler.GetTriggersOfJob(jobKey);

        return new List<JobNextExecutionDto>
        {
            new JobNextExecutionDto
            {
                JobName = jobKey.Name,
                JobGroup = jobKey.Group,
                NextExecutions = triggers.Select(t => new TriggerExecutionDto
            {
                TriggerName = t.Key.Name,
                TriggerGroup = t.Key.Group,
                NextExecutions = t is ICronTrigger cronTrigger
                    ? CronHelper.GetUpcomingExecutions(cronTrigger.CronExpressionString, DefaultExecutionCount)
                    : new List<DateTimeOffset> { t.GetNextFireTimeUtc()?.ToLocalTime() ?? DateTimeOffset.MinValue }
            }).ToList()
            }
        };
    }

    /// <summary>
    /// Retrieves the current status of the specified job, based on its triggers and execution state.
    /// </summary>
    /// <param name="scheduler">Quartz scheduler.</param>
    /// <param name="jobKey">Job key to check.</param>
    /// <returns>The calculated job status.</returns>
    private static async Task<QuartzJobStatus> GetJobStatus(IScheduler scheduler, JobKey jobKey)
    {
        var triggers = await scheduler.GetTriggersOfJob(jobKey);
        var states = await Task.WhenAll(triggers.Select(t => scheduler.GetTriggerState(t.Key)));

        var runningJobs = await scheduler.GetCurrentlyExecutingJobs();
        var isRunning = runningJobs.Any(x =>
            x.JobDetail.Key.Name == jobKey.Name &&
            x.JobDetail.Key.Group == jobKey.Group);

        return GetJobStatus(states, isRunning);
    }

    /// <summary>
    /// Determines the job status based on trigger states and whether the job is currently running.
    /// </summary>
    /// <param name="states">Trigger states for the job.</param>
    /// <param name="isRunning">Indicates if the job is currently running.</param>
    private static QuartzJobStatus GetJobStatus(TriggerState[] states, bool isRunning)
    {
        if (isRunning)
            return QuartzJobStatus.Running;

        if (states.Length == 0)
            return QuartzJobStatus.Unknown;

        return states.Distinct().Count() switch
        {
            1 => states[0] switch
            {
                TriggerState.Paused => QuartzJobStatus.Paused,
                TriggerState.Normal => QuartzJobStatus.Active,
                _ => QuartzJobStatus.Unknown,
            },
            > 1 => QuartzJobStatus.Mixed,
            _ => QuartzJobStatus.Unknown,
        };
    }
}
