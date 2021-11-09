using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nest;

namespace OutOfSchool.WebApi.Services
{
    public class ElasticPinger : IHostedService, IDisposable
    {
        private readonly ILogger<ElasticPinger> logger;
        private Timer timer;
        private ElasticClient elasticClient;

        public ElasticPinger(ILogger<ElasticPinger> logger, ElasticClient client)
        {
            this.logger = logger;
            this.elasticClient = client;
        }

        public bool IsHealthy { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Service started pinging");
            timer = new Timer(
                cb => IsHealthy = elasticClient.Ping().IsValid,
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
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