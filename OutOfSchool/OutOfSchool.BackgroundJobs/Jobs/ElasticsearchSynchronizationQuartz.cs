using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.CompetitiveEvents;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class ElasticsearchSynchronizationQuartz : IJob
{
    private readonly IServiceProvider services;

    public ElasticsearchSynchronizationQuartz(
        IServiceProvider services)
    {
        this.services = services;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = services.CreateScope();
        var elasticHealthService = scope.ServiceProvider.GetRequiredService<IElasticsearchHealthService>();
        if (elasticHealthService.IsHealthy)
        {
            var config = scope.ServiceProvider.GetRequiredService<IOptions<ElasticConfig>>();

            var elasticsearchWorkshopSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService<IWorkshopService, Workshop>>();

            var elasticsearchCompetitiveEventSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();

            await elasticsearchWorkshopSynchronizationService.Synchronize(config.Value.WorkshopIndexName, context.CancellationToken).ConfigureAwait(false);
            await elasticsearchCompetitiveEventSynchronizationService.Synchronize(config.Value.CompetitiveEventIndexName, context.CancellationToken).ConfigureAwait(false);
        }
    }
}