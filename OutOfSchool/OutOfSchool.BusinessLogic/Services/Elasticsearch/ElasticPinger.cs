using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;

namespace OutOfSchool.BusinessLogic.Services;

public class ElasticPinger : IElasticsearchHealthService, IHostedService, IDisposable
{
    private readonly ILogger<ElasticPinger> logger;
    private Timer timer;
    private ElasticsearchClient elasticClient;
    private ElasticConfig elasticConfig;

    public ElasticPinger(
        ILogger<ElasticPinger> logger,
        ElasticsearchClient client,
        IOptions<ElasticConfig> elasticOptions)
    {
        this.logger = logger;
        elasticClient = client;
        elasticConfig = elasticOptions.Value;
    }

    public bool IsHealthy { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Service started pinging");
        timer = new Timer(
            async (cb) =>
            {
                try
                {
                    IsHealthy =
                    (await elasticClient.PingAsync().ConfigureAwait(false)).IsValidResponse;
                }
                catch (Exception ex)
                {
                    IsHealthy = false;
                    logger.LogWarning($"Error while pinging Elasticsearch: {ex.Message}");
                }
            },
            null,
            TimeSpan.Zero,
            TimeSpan.FromSeconds(elasticConfig.PingIntervalSeconds));
        logger.LogInformation("Service did ping");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}