using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class AikomApiClientServiceExtensions
{
    public static AikomApiClientConfig? RegisterAikomApiClient(
        this IServiceCollection services, 
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var aikomConfiguration = configuration
            .GetSection(AikomApiClientConfig.Name)
            .Get<AikomApiClientConfig>();
        services.Configure<AikomApiClientConfig>(configuration.GetSection(AikomApiClientConfig.Name));

        if (aikomConfiguration.Enable)
        {
            services.AddTransient<ICommunicationService, CommunicationService>();
            services.AddTransient<IAikomApiService, AikomApiService>();
            services.AddTransient<IAikomProviderService, AikomProviderService>();
        }
        else if (environment.IsDevelopment())
        {
            services.AddTransient<IAikomProviderService, FakeAikomProviderService>();
        }
        else
        {
            throw new InvalidOperationException(
                "Aikom API client is not enabled and environment is not Development. " +
                "Either enable the client in configuration or switch to Development environment to use fake implementation.");
        }
        
        return aikomConfiguration;
    }
}