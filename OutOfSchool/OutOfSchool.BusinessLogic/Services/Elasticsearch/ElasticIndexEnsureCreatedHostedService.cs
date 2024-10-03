using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.Elasticsearch;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class ElasticIndexEnsureCreatedHostedService : IHostedService
{
    private readonly IServiceProvider services;
    private readonly ElasticsearchClient client;
    private readonly string indexName;
    private readonly int checkConnectivityDelayMs;
    private readonly int connectionWaitingTimeSec;
    private readonly ElasticsearchWorkshopConfiguration configurator = new();
    private readonly ILogger<ElasticIndexEnsureCreatedHostedService> logger;

    public ElasticIndexEnsureCreatedHostedService(
        IServiceProvider services,
        ElasticsearchClient client,
        IOptions<ElasticConfig> elasticOptions,
        ILogger<ElasticIndexEnsureCreatedHostedService> logger)
    {
        this.services = services;
        this.client = client;
        this.logger = logger;
        var config = elasticOptions?.Value
            ?? throw new ArgumentNullException(nameof(elasticOptions));
        indexName = config.WorkshopIndexName
            ?? throw new ArgumentNullException(nameof(elasticOptions), "WorkshopIndexName is null");
        checkConnectivityDelayMs = config.CheckConnectivityDelayMs;
        connectionWaitingTimeSec = config.ConnectionWaitingTimeSec;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Elastic index ensure operation was cancelled");
            return;
        }

        var startTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        using var scope = services.CreateScope();
        var elasticHealthService = scope.ServiceProvider.GetService<IElasticsearchHealthService>();
        while (!elasticHealthService.IsHealthy
                && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime < connectionWaitingTimeSec))
        {
            logger.LogInformation("Waiting for Elastic connection");
            await Task.Delay(checkConnectivityDelayMs).ConfigureAwait(false);
        }

        if (elasticHealthService.IsHealthy)
        {
            var existsResponse = await client.Indices.ExistsAsync(indexName).ConfigureAwait(false);
            if (existsResponse.ApiCallDetails.HttpStatusCode == StatusCodes.Status404NotFound)
            {
                await client.Indices.CreateAsync(indexName, configurator.Configure())
                    .ConfigureAwait(false);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
