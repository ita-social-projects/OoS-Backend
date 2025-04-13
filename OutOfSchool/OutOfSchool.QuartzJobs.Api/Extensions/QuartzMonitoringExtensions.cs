using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.QuartzJobs.Api.Configuration;
using OutOfSchool.QuartzJobs.Api.Listeners;
using OutOfSchool.QuartzJobs.Api.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using StackExchange.Redis;

namespace OutOfSchool.QuartzJobs.Api.Extensions;

public static class QuartzMonitoringExtensions
{
    public static void AddQuartzMonitoringListener(this IServiceCollectionQuartzConfigurator quartz)
    {
        quartz.AddJobListener<JobMonitoringListener>();
    }

    public static IServiceCollection AddQuartzMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        // Add QuartzMonitoring configuration
        services.Configure<QuartzMonitoringOptions>(configuration.GetSection("QuartzMonitoring"));

        // Register Logger and Listener for Quartz  
        services.AddSingleton<IJobExecutionLogger, RedisJobExecutionLogger>();
        services.AddSingleton<IJobListener, JobMonitoringListener>();

        // Redis connection
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<QuartzMonitoringOptions>>().Value;

            var redis = options.Redis!;

            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { $"{redis.Server}:{redis.Port}" },
                Password = string.IsNullOrWhiteSpace(redis.Password) ? null : redis.Password,
                AbortOnConnectFail = false
            };

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapQuartzMonitoringApi(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/monitoring")
            .WithTags("Quartz Monitoring");

        group.MapGet("/jobs", async ([FromServices] ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var jobs = new List<object>();

            foreach (var jobKey in jobKeys)
            {
                var detail = await scheduler.GetJobDetail(jobKey);
                jobs.Add(new
                {
                    jobKey.Name,
                    jobKey.Group,
                    detail.Description,
                    JobType = detail.JobType.FullName
                });
            }

            return Results.Ok(jobs);
        });

        endpoints.MapGet("/jobs/{jobName}", async (string jobName, [FromServices] ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var jobKey = jobKeys.FirstOrDefault(k => k.Name == jobName);
            if (jobKey is null)
                return Results.NotFound();

            var detail = await scheduler.GetJobDetail(jobKey);
            var triggers = await scheduler.GetTriggersOfJob(jobKey);

            return Results.Ok(new
            {
                jobKey.Name,
                jobKey.Group,
                detail.Description,
                JobType = detail.JobType.FullName,
                Triggers = triggers.Select(t => new {
                    TriggerKey = t.Key,
                    NextFireTimeUtc = t.GetNextFireTimeUtc(),
                    Cron = t is ICronTrigger cron ? cron.CronExpressionString : null
                })
            });
        })
        .WithName("GetJobDetails");


        group.MapGet("/jobs/{jobName}/history", async (string jobName, IJobExecutionLogger logger) =>
        {
            var history = await logger.GetHistoryAsync(jobName);
            return Results.Ok(history);
        });

        group.MapGet("/jobs/{jobName}/errors", async (string jobName, IJobExecutionLogger logger) =>
        {
            var failed = await logger.GetFailedAsync(jobName);
            return Results.Ok(failed);
        });

        group.MapGet("/jobs/{jobName}/next", async (string jobName, [FromServices] ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
            var jobKey = jobKeys.FirstOrDefault(k => k.Name == jobName);
            if (jobKey is null)
                return Results.NotFound();

            var triggers = await scheduler.GetTriggersOfJob(jobKey);
            var upcoming = triggers
                .Select(t => new {
                    Trigger = t.Key,
                    NextFireTimeUtc = t.GetNextFireTimeUtc(),
                    Cron = t is ICronTrigger cron ? cron.CronExpressionString : null
                });

            return Results.Ok(upcoming);
        });

        group.MapGet("/jobs/status", async ([FromServices] ISchedulerFactory schedulerFactory) =>
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var currentlyExecuting = await scheduler.GetCurrentlyExecutingJobs();

            var status = currentlyExecuting.Select(ctx => new
            {
                Job = ctx.JobDetail.Key.Name,
                ctx.JobDetail.Key.Group,
                StartedAt = ctx.FireTimeUtc,
                Trigger = ctx.Trigger.Key.Name,
                TriggerGroup = ctx.Trigger.Key.Group
            });

            return Results.Ok(status);
        });

        return endpoints;
    }
}

