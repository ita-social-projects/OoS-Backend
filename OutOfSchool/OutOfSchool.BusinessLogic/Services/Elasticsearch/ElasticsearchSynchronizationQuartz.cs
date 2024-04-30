﻿using Quartz;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

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
        var elasticPinger = scope.ServiceProvider.GetRequiredService<ElasticPinger>();
        if (elasticPinger.IsHealthy)
        {
            var elasticsearchSynchronizationService =
                scope.ServiceProvider
                    .GetRequiredService<IElasticsearchSynchronizationService>();

            await elasticsearchSynchronizationService.Synchronize(context.CancellationToken).ConfigureAwait(false);
        }
    }
}