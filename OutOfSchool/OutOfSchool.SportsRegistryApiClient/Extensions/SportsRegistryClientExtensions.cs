using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.SportsRegistryApiClient.Config;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Services;

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

        if (sportConfiguration is null)
        {
            throw new InvalidOperationException(
            $"Configuration section '{SportsRegistryApiClientConfig.Name}' is missing or malformed.");
        }
        services.Configure<SportsRegistryApiClientConfig>(configuration.GetSection(SportsRegistryApiClientConfig.Name));

        if (sportConfiguration.Enable)
        {
            services.TryAddTransient<ICommunicationService, CommunicationService>();
            services.TryAddTransient<ISportsRegistryApiService, SportsRegistryApiService>();
            services.TryAddTransient<ISportsRegistrySectionProvider, SportsRegistrySectionProvider>();
            services.TryAddTransient<ISportsRegistryDictionaryProvider, SportsRegistryDictionaryProvider>();
        }
        else
        {
            services.TryAddSingleton<ISportsRegistrySectionProvider, DisabledSportsRegistryStub>();
        }
        
        return sportConfiguration;
    }
}