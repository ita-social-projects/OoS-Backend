using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Options;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.GRPC;

namespace OutOfSchool.GRPC
{
    /// <summary>
    /// Implements the interface for common operations with GRPC.
    /// </summary>
    public class GRPCCommonService : IGRPCCommonService
    {
        private readonly GRPCConfig gRPCConfig;

        /// <summary>
        /// Initializes a new instance of the <see cref="GRPCCommonService"/> class.
        /// </summary>
        /// <param name="gRPCConfig">GRPC configuration parameters.</param>
        public GRPCCommonService(
            IOptions<GRPCConfig> gRPCConfig)
        {
            try
            {
                this.gRPCConfig = gRPCConfig.Value;
                IsEnabled = this.gRPCConfig.Enabled;
            }
            catch (OptionsValidationException)
            {
                IsEnabled = false;
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled { get; }

        /// <inheritdoc/>
        public GrpcChannel CreateAuthenticatedChannel(string token)
        {
            var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                if (!string.IsNullOrEmpty(token))
                {
                    metadata.Add("Authorization", $"Bearer {token}");
                }

                return Task.CompletedTask;
            });

            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = gRPCConfig.ChannelMaxAttempts,
                    InitialBackoff = gRPCConfig.ChannelInitialBackoffInterval,
                    MaxBackoff = gRPCConfig.ChannelMaxBackoffInterval,
                    BackoffMultiplier = gRPCConfig.ChannelBackoffMultiplier,
                    RetryableStatusCodes = { StatusCode.Unavailable },
                },
            };

            var channel = GrpcChannel.ForAddress(
                $"{gRPCConfig.Server}:{gRPCConfig.Port}",
                new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Create(new SslCredentials(), credentials),
                    ServiceConfig = new ServiceConfig { MethodConfigs = { defaultMethodConfig } },
                });

            return channel;
        }
    }
}
