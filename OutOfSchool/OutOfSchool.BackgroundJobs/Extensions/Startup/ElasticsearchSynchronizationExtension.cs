using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.BackgroundJobs.Jobs;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.Elasticsearch;
using OutOfSchool.Common.QuartzConstants;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Extensions.Startup;

public static class ElasticsearchSynchronizationExtension
{
    /// <summary>
    /// Adds all essential methods to synchronize elasticsearch data with the main database.
    /// </summary>
    /// <param name="quartz">Quartz Configurator.</param>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">App configuration.</param>
    /// <param name="elasticsearchSynchronizationSchedulerConfig">Scheduler config.</param>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static void AddElasticsearchSynchronization(
        this IServiceCollectionQuartzConfigurator quartz,
        IServiceCollection services,
        IConfiguration configuration,
        Action<OptionsBuilder<ElasticsearchSynchronizationSchedulerConfig>>?
            elasticsearchSynchronizationSchedulerConfig = null)
    {
        _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

        elasticsearchSynchronizationSchedulerConfig ??= builder =>
            builder.Bind(configuration.GetSection(ElasticsearchSynchronizationSchedulerConfig.SectionName));

        services.AddTransient<IElasticsearchSynchronizationService<IWorkshopService, Workshop>, WorkshopSynchronizationService>();
        services.AddTransient<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>, CompetitiveEventSynchronizationService>();

        ArgumentNullException.ThrowIfNull(elasticsearchSynchronizationSchedulerConfig);

        var elasticsearchSynchronizationSchedulerConfigBuilder =
            services.AddOptions<ElasticsearchSynchronizationSchedulerConfig>();
        elasticsearchSynchronizationSchedulerConfig(elasticsearchSynchronizationSchedulerConfigBuilder);

        var elasticSynchronizationSchedulerConfig = configuration
            .GetSection(ElasticsearchSynchronizationSchedulerConfig.SectionName)
            .Get<ElasticsearchSynchronizationSchedulerConfig>();

        var workshopSyncJobKey = new JobKey(JobConstants.ElasticSearchWorkshopSynchronization, GroupConstants.ElasticSearch);

        quartz.AddJob<ElasticsearchWorkshopSynchronizationQuartzJob>(j => j
            .WithIdentity(workshopSyncJobKey)
            .WithDescription("Synchronizes workshop data to Elasticsearch index."));
        // TODO: rewrite as a cron trigger
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.ElasticSearchWorkshopSynchronization, GroupConstants.ElasticSearch)
            .ForJob(workshopSyncJobKey)
            .StartNow()
            .WithSimpleSchedule(x =>
                x.WithInterval(TimeSpan.FromMilliseconds(elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds))
                 .RepeatForever())
            .WithDescription($"Runs repeatedly every {elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds} ms."));

        var competitiveEventSyncJobKey = new JobKey(JobConstants.ElasticSearchCompetitiveEventSynchronization, GroupConstants.ElasticSearch);

        quartz.AddJob<ElasticsearchCompetitiveEventSynchronizationQuartzJob>(j => j
            .WithIdentity(competitiveEventSyncJobKey)
            .WithDescription("Synchronizes competitive event data to Elasticsearch index."));
        // TODO: rewrite as a cron trigger
        quartz.AddTrigger(t => t
            .WithIdentity(JobTriggerConstants.ElasticSearchCompetitiveEventSynchronization, GroupConstants.ElasticSearch)
            .ForJob(competitiveEventSyncJobKey)
            .StartNow()
            .WithSimpleSchedule(x =>
                x.WithInterval(TimeSpan.FromMilliseconds(elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds))
                 .RepeatForever())
            .WithDescription($"Runs repeatedly every {elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds} ms."));
    }
}