using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.AikomApiClient.Config;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class AikomApiClientServiceExtensions
{
    public static AikomApiClientConfig? RegisterAikomApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        var aikomConfiguration = configuration
            .GetSection(AikomApiClientConfig.Name)
            .Get<AikomApiClientConfig>();
        services.Configure<AikomApiClientConfig>(configuration.GetSection(AikomApiClientConfig.Name));
        
        services.AddTransient<IAikomApiService, AikomApiService>();
        services.AddTransient<IAikomProviderService, AikomProviderService>();
        
        return aikomConfiguration;
    }
}