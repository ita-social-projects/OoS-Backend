using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.Common.Communication;
using OutOfSchool.SportsRegistryApiClient.Config;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Services;
using OutOfSchool.Common.Communication.ICommunication;

namespace OutOfSchool.SportsRegistryApiClient.Extensions;

public static class SportsRegistryClientExtensions
{
    public static SportsRegistryApiClientConfig? AddSportsRegistryClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sportConfiguration = configuration
            .GetSection(SportsRegistryApiClientConfig.Name)
            .Get<SportsRegistryApiClientConfig>();
        services.Configure<SportsRegistryApiClientConfig>(configuration.GetSection(SportsRegistryApiClientConfig.Name));
        
        services.AddTransient<ICommunicationService, CommunicationService>();
        services.AddTransient<ISportsRegistryApiService, SportsRegistryApiService>();
        services.AddTransient<ISportsRegistryProviderService, SportsRegistryProviderService>();

        return sportConfiguration;
    }
}