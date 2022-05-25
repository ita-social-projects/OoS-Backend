using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Elasticsearch;
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
                var jobKey = new JobKey("elasticsearchJob");

                services.AddQuartz(q =>
                {
                    q.SchedulerId = "Elasticsearch";
                    q.SchedulerName = "Elasticsearch";
                    q.SetProperty("quartz.serializer.type", "json");
                    q.SetProperty("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                    q.SetProperty("quartz.jobStore.dataSource", "default");
                    q.SetProperty("quartz.dataSource.default.provider", "MySql");
                    q.SetProperty("quartz.dataSource.default.connectionString", elasticSynchronizationSchedulerConfig.QuartzConnectionString);
                    q.SetProperty("quartz.jobStore.clustered", "true");
                    q.SetProperty("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                    q.SetProperty("quartz.jobStore.useProperties", "true");
                    q.UseMicrosoftDependencyInjectionJobFactory();
                    q.AddJob<ElasticsearchSynchronizationQuartz>(j => j.WithIdentity(jobKey));
                    q.AddTrigger(t => t
                       .WithIdentity("elasticsearchJobTrigger")
                       .ForJob(jobKey)
                       .StartNow()
                       .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMilliseconds(elasticSynchronizationSchedulerConfig.DelayBetweenTasksInMilliseconds)).RepeatForever()));
                });

                services.AddQuartzServer(options =>
                {
                    options.WaitForJobsToComplete = true;
                });
            }
            else
            {
                services.AddHostedService<ElasticsearchSynchronizationHostedService>();
            }

            return services;
        }
    }
}
