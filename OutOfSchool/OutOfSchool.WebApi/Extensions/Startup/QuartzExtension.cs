using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.Elasticsearch;
using OutOfSchool.WebApi.Util;
using Quartz;

namespace OutOfSchool.WebApi.Extensions.Startup
{
    public static class QuartzExtension
    {
        /// <summary>
        /// Adds default Quartz.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">App configuration.</param>
        /// <returns><see cref="IServiceCollection"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
        public static IServiceCollection AddDefaultQuartz(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            _ = services ?? throw new ArgumentNullException(nameof(services));

            services.AddQuartz(q =>
            {
                q.SchedulerId = "DefaultAppQuartz";
                q.SchedulerName = "DefaultAppQuartz";
                q.SetProperty("quartz.serializer.type", "json");
                q.SetProperty("quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz");
                q.SetProperty("quartz.jobStore.dataSource", "default");
                q.SetProperty("quartz.dataSource.default.provider", "MySql");
                q.SetProperty(
                    "quartz.dataSource.default.connectionString",
                    configuration.GetMySqlConnectionString<QuartzConnectionOptions>(
                        "QuartzConnection",
                        options => new MySqlConnectionStringBuilder
                        {
                            Server = options.Server,
                            Port = options.Port,
                            UserID = options.UserId,
                            Password = options.Password,
                            Database = options.Database,
                        }));
                q.SetProperty("quartz.jobStore.clustered", "true");
                q.SetProperty("quartz.jobStore.driverDelegateType", "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz");
                q.SetProperty("quartz.jobStore.useProperties", "true");
                q.UseMicrosoftDependencyInjectionJobFactory();

                foreach (var quartzAction in QuartzPool.QuartzConfigActions)
                {
                    quartzAction?.Invoke(q);
                }
            });

            services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });

            QuartzPool.ClearAll();

            return services;
        }
    }
}