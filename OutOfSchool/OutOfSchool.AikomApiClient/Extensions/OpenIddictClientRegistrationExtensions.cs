using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OutOfSchool.AikomApiClient.Config;

namespace OutOfSchool.AikomApiClient.Extensions;

public static class OpenIddictClientRegistrationExtensions
{
    public static OpenIddictClientBuilder AddAikomOpenIddictClientRegistration(
        this OpenIddictClientBuilder clientBuilder, AikomApiClientConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        if (!config.Enable)
        {
            return clientBuilder;
        }

        return clientBuilder.AddRegistration(new OpenIddictClientRegistration
        {
            ProviderName = "aikom",
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
