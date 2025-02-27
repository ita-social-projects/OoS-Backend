using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models.CompetitiveEvents;
using Quartz;

namespace OutOfSchool.BackgroundJobs.Jobs;

public class ElasticsearchCompetitiveEventSynchronizationQuartzJob : IJob
{
    private readonly IServiceProvider services;

    public ElasticsearchCompetitiveEventSynchronizationQuartzJob(
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

            var elasticsearchCompetitiveEventSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService<ICompetitiveEventService, CompetitiveEvent>>();
            
            await elasticsearchCompetitiveEventSynchronizationService.Synchronize(
                config.Value.CompetitiveEventIndexName, context.CancellationToken).ConfigureAwait(false);
        }
    }
}