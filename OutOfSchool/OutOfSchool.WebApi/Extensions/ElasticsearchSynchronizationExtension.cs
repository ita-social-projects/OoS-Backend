using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MySqlConnector;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.WebApi.Common.QuartzConstants;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Elasticsearch;
using OutOfSchool.WebApi.Util;
using Quartz;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchSynchronizationExtension
    {
        /// <summary>
        /// Adds all essential methods to synchronize elasticsearch data with the main database.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">App configuration.</param>
        /// <param name="elasticsearchSynchronizationSchedulerConfig">Scheduler config.</param>
        /// <returns><see cref="IServiceCollection"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
        public static IServiceCollection AddElasticsearchSynchronization(
            this IServiceCollection services,
            IConfiguration configuration,
            Action<OptionsBuilder<ElasticsearchSynchronizationSchedulerConfig>> elasticsearchSynchronizationSchedulerConfig = null)
        {
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            elasticsearchSynchronizationSchedulerConfig ??= builder =>
                builder.Bind(configuration.GetSection(ElasticsearchSynchronizationSchedulerConfig.SectionName));

            services.AddTransient<IElasticsearchSynchronizationService, ElasticsearchSynchronizationService>();

            if (elasticsearchSynchronizationSchedulerConfig == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchSynchronizationSchedulerConfig));
            }

            var elasticsearchSynchronizationSchedulerConfigBuilder = services.AddOptions<ElasticsearchSynchronizationSchedulerConfig>();
            elasticsearchSynchronizationSchedulerConfig(elasticsearchSynchronizationSchedulerConfigBuilder);

            var elasticSynchronizationSchedulerConfig = configuration.GetSection(ElasticsearchSynchronizationSchedulerConfig.SectionName).Get<ElasticsearchSynchronizationSchedulerConfig>();

            if (elasticSynchronizationSchedulerConfig.UseQuartz)
            {
                var jobKey = new JobKey(JobConstants.ElasticSearchSynchronization, GroupConstants.ElasticSearch);

                QuartzPool.AddJob<ElasticsearchSynchronizationQuartz>(j => j.WithIdentity(jobKey));
                QuartzPool.AddTrigger(t => t
                       .WithIdentity(JobTriggerConstants.ElasticSearchSynchronization, GroupConstants.ElasticSearch)
                       .ForJob(jobKey)
                       .StartNow()
                       .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMilliseconds(elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds)).RepeatForever()));
            }
            else
            {
                services.AddHostedService<ElasticsearchSynchronizationHostedService>();
            }

            return services;
        }
    }
}
