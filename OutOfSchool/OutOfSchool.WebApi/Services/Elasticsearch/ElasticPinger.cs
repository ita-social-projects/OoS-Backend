using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using OutOfSchool.WebApi.Config;

namespace OutOfSchool.WebApi.Services
{
    public class ElasticPinger : IHostedService, IDisposable
    {
        private readonly ILogger<ElasticPinger> logger;
        private Timer timer;
        private ElasticClient elasticClient;
        private ElasticConfig elasticConfig;

        public ElasticPinger(
            ILogger<ElasticPinger> logger,
            ElasticClient client,
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
                cb => IsHealthy = elasticClient.Ping().IsValid,
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
}