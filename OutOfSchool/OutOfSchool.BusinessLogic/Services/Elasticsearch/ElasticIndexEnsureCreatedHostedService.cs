using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Config.Elasticsearch;

namespace OutOfSchool.BusinessLogic.Services.Elasticsearch;

public class ElasticIndexEnsureCreatedHostedService(
    IServiceProvider services,
    ElasticsearchClient client,
    IOptions<ElasticConfig> elasticOptions,
    ILogger<ElasticIndexEnsureCreatedHostedService> logger) : IHostedService
{
    private const int CheckConnectivityDelayMs = 10000;
    private const int Minute = 60;

    private readonly IServiceProvider services = services;
    private readonly ElasticsearchClient client = client;
    private readonly string indexName =
        elasticOptions?.Value is not null
            ? elasticOptions.Value.WorkshopIndexName
            : throw new ArgumentNullException(nameof(elasticOptions));

    private readonly ElasticsearchWorkshopConfiguration configurator = new();

    private readonly ILogger<ElasticIndexEnsureCreatedHostedService> logger = logger;

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
                && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startTime < Minute))
        {
            logger.LogInformation("Waiting for Elastic connection");
            await Task.Delay(CheckConnectivityDelayMs);
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
