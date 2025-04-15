using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.QuartzJobs.Api.Configuration;
using OutOfSchool.QuartzJobs.Api.Listeners;
using OutOfSchool.QuartzJobs.Api.Logging;
using OutOfSchool.QuartzJobs.Api.Services;
using Quartz;
using StackExchange.Redis;

namespace OutOfSchool.QuartzJobs.Api.Extensions;

/// <summary>
/// Provides extension methods to register Quartz monitoring services and endpoints.
/// </summary>
public static class QuartzMonitoringExtensions
{
    /// <summary>
    /// Registers the job execution listener for monitoring Quartz jobs.
    /// </summary>
    /// <param name="quartz">Quartz service configurator.</param>
    public static void AddQuartzMonitoringListener(this IServiceCollectionQuartzConfigurator quartz)
    {
        quartz.AddJobListener<JobMonitoringListener>();
    }

    /// <summary>
    /// Registers Quartz monitoring services, configuration, Redis connection, and logger.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddQuartzMonitoring(this IServiceCollection services, IConfiguration configuration)
    {
        // Add QuartzMonitoring configuration
        services.Configure<QuartzMonitoringOptions>(configuration.GetSection("QuartzMonitoring"));
        services.AddSingleton<IQuartzMonitoringService, QuartzMonitoringService>();

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
}

