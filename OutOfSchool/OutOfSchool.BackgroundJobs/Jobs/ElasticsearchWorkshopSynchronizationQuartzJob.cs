using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class ElasticsearchWorkshopSynchronizationQuartzJob : IJob
{
    private readonly IServiceProvider services;

    public ElasticsearchWorkshopSynchronizationQuartzJob(
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

            await elasticsearchWorkshopSynchronizationService.Synchronize(
                config.Value.WorkshopIndexName, context.CancellationToken).ConfigureAwait(false);            
        }
    }
}