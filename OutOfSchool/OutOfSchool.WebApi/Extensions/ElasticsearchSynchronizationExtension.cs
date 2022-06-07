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
        public static IServiceCollection AddElasticsearchSynchronization(
            this IServiceCollection services,
            Action<OptionsBuilder<ElasticsearchSynchronizationSchedulerConfig>> elasticsearchSynchronizationSchedulerConfig,
            IConfiguration configuration)
        {
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
