using OutOfSchool.AuthorizationServer.Config;
using Quartz;

namespace OutOfSchool.AuthorizationServer.Extensions;

public static class QuartzExtension
{
    /// <summary>
    /// Adds default Quartz.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">App configuration.</param>
    /// <param name="quartzConnectionString">Connection string key for Quartz.</param>
    /// <param name="configureJobs">Expose Quartz Configurator to Configure Jobs.</param>
    /// <returns><see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Whenever the services collection is null.</exception>
    public static async Task<IServiceCollection> AddDefaultQuartz(
        this IServiceCollection services,
        IConfiguration configuration,
        string quartzConnectionString = "QuartzConnection",
        Action<IServiceCollectionQuartzConfigurator>? configureJobs = null)
    {
        _ = services ?? throw new ArgumentNullException(nameof(services));

        services.AddQuartz(q =>
        {
            q.SchedulerId = DefaultQuartzConfig.DefaultId;
            q.SchedulerName = DefaultQuartzConfig.DefaultName;

            q.UsePersistentStore(s =>
            {
                s.UseProperties = true;
                s.UseMySql(sqlServer =>
                {
                    sqlServer.ConnectionString = configuration.GetMySqlConnectionString<QuartzConnectionOptions>(
                        quartzConnectionString,
                        options => new MySqlConnectionStringBuilder
                        {
                            Server = options.Server,
                            Port = options.Port,
                            UserID = options.UserId,
                            Password = options.Password,
                            Database = options.Database,
                        });
                });
                s.UseSystemTextJsonSerializer();
                s.UseClustering();
            });

            q.UseTimeZoneConverter();

            configureJobs?.Invoke(q);
        });

        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

        return services;
    }
}