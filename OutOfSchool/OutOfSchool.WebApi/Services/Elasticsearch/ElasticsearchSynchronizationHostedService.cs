using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Services;

[Obsolete("Using Quartz as scheduler")]
public class ElasticsearchSynchronizationHostedService : BackgroundService
{
    private readonly ILogger<ElasticsearchSynchronizationHostedService> logger;

    public ElasticsearchSynchronizationHostedService(
        IServiceProvider services,
        ILogger<ElasticsearchSynchronizationHostedService> logger)
    {
        Services = services;
        this.logger = logger;
    }

    public IServiceProvider Services { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Elasticsearch synchronization hosted service running.");

        await Synchronize(cancellationToken).ConfigureAwait(false);
    }

    private async Task Synchronize(CancellationToken cancellationToken)
    {
        logger.LogInformation("Elasticsearch synchronization started.");

        using (var scope = Services.CreateScope())
        {
            var elasticsearchSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService>();

            await elasticsearchSynchronizationService.Synchronize(cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Elasticsearch synchronization hosted service is stopping.");

        await base.StopAsync(cancellationToken).ConfigureAwait(false);
    }
}