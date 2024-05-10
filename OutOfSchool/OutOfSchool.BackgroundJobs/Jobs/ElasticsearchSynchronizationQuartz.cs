using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.BusinessLogic.Services;
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
            var elasticsearchSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService>();

            await elasticsearchSynchronizationService.Synchronize(context.CancellationToken).ConfigureAwait(false);
        }
    }
}