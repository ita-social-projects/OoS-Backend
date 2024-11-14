using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OutOfSchool.AikomApiClient.Config;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAikomApiClient(
        this IServiceCollection services, AikomApiClientConfig config)
    {
        if (services is null || config is null)
        {
            throw new ArgumentNullException($"One of the parameters({services} or {config}) was not set to instance.");
        }

        services.AddOpenIddict().AddClient(options =>
        {
            options.AllowClientCredentialsFlow();
            options.DisableTokenStorage();
            options.UseSystemNetHttp();
            options.UseAspNetCore();
            options.AddRegistration(new OpenIddictClientRegistration
            {
                Issuer = new Uri(config.ApiUrl, UriKind.Absolute),
                ClientId = config.ClientId,
                ClientSecret = config.ClientSecret,
                Configuration = new()
                {
                    TokenEndpoint = new Uri(config.TokenEndpoint, UriKind.Absolute),
                    AuthorizationEndpoint = new Uri(config.AuthorizationEndpoint, UriKind.Absolute),
                    GrantTypesSupported = { OpenIddictConstants.GrantTypes.ClientCredentials },
                    TokenEndpointAuthMethodsSupported = { OpenIddictConstants.ClientAuthenticationMethods.ClientSecretPost }
                }
            });
        });

        services.AddTransient<IAikomApiService, AikomApiService>();
        
        return services;

    }
}
