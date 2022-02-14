using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Extensions
{
    public static class ElasticsearchSynchronizationExtension
    {
        public static IServiceCollection AddElasticsearchSynchronization(
            this IServiceCollection services,
            Action<OptionsBuilder<ElasticsearchSynchronizationSchedulerConfig>> elasticsearchSynchronizationSchedulerConfig)
        {
            services.AddHostedService<ElasticsearchSynchronizationHostedService>();
            services.AddTransient<IElasticsearchSynchronizationService, ElasticsearchSynchronizationService>();
            if (elasticsearchSynchronizationSchedulerConfig == null)
            {
                throw new ArgumentNullException(nameof(elasticsearchSynchronizationSchedulerConfig));
            }

            var elasticsearchSynchronizationSchedulerConfigBuilder = services.AddOptions<ElasticsearchSynchronizationSchedulerConfig>();
            elasticsearchSynchronizationSchedulerConfig(elasticsearchSynchronizationSchedulerConfigBuilder);
            return services;
        }
    }
}
