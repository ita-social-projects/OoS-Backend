
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OutOfSchool.SportsRegistryApiClient.Config;

namespace OutOfSchool.SportsRegistryApiClient.Extensions;

public static class OpenIddictClientRegistrationExtensions
{
    public static OpenIddictClientBuilder AddSportsRegistryOpenIddictClientRegistration(
        this OpenIddictClientBuilder clientBuilder, SportsRegistryApiClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.Enable)
        {
            return clientBuilder;
        }

        return clientBuilder.AddRegistration(new OpenIddictClientRegistration
        {
            ProviderName = "sportsregistry",
            Issuer = new Uri(config.ApiUrl, UriKind.Absolute),
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            Configuration = new()
            {
                TokenEndpoint = new Uri(config.TokenEndpoint, UriKind.Absolute),
                GrantTypesSupported = { OpenIddictConstants.GrantTypes.ClientCredentials },
                TokenEndpointAuthMethodsSupported = { OpenIddictConstants.ClientAuthenticationMethods.ClientSecretPost }
            }
        });
    }
}