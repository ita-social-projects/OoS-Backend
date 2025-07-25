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
    public static IServiceCollection AddSportsRegistryClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<SportsRegistryApiClientConfig>(
            configuration.GetSection(SportsRegistryApiClientConfig.SectionName));

        services.AddTransient<ICommunicationService, CommunicationService>();
        services.AddTransient<ISportsRegistryApiService, SportsRegistryApiService>();

        return services;
    }
}