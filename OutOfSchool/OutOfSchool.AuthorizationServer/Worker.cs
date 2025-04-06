using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OutOfSchool.AuthCommon.Config;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OutOfSchool.AuthorizationServer;

// TODO: Use client info from settings
public class Worker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public Worker(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await RegisterApplicationsAsync(scope.ServiceProvider);
        await RegisterScopesAsync(scope.ServiceProvider);

        static async Task RegisterApplicationsAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();
            var options = provider.GetRequiredService<IOptions<AuthorizationServerConfig>>().Value;
            foreach (var (name, client) in options.OpenIdClients)
            {
                var clientId = client.ClientId ?? name;
                if (await manager.FindByClientIdAsync(clientId) is null)
                {
                    OpenIddictApplicationDescriptor descriptor;
                    if (client.IsIntrospection)
                    {
                        descriptor = new()
                        {
                            ClientId = clientId,
                            ClientSecret = options.IntrospectionSecret,
                            Permissions =
                            {
                                Permissions.Endpoints.Introspection,
                            },
                        };
                    }
                    else
                    {
                        if (client.ClientSecret is not null)
                        {
                            descriptor = new()
                            {
                                ClientId = clientId,
                                DisplayName = client.DisplayName,
                                ClientSecret = client.ClientSecret,
                                Permissions =
                                {
                                    Permissions.Endpoints.Token,
                                    Permissions.GrantTypes.ClientCredentials,
                                    Permissions.Scopes.Profile,
                                    Permissions.Prefixes.Scope + Constants.OpenIddictScopes.ExternalExportRead,
                                },
                            };
                        }
                        else
                        {
                            descriptor = new OpenIddictApplicationDescriptor
                            {
                                ClientId = clientId,
                                ConsentType = ConsentTypes.Implicit,
                                DisplayName = client.DisplayName,
                                Permissions =
                                {
                                    Permissions.Endpoints.Authorization,
                                    Permissions.Endpoints.EndSession,
                                    Permissions.Endpoints.Token,
                                    Permissions.Endpoints.Revocation,
                                    Permissions.GrantTypes.AuthorizationCode,
                                    Permissions.GrantTypes.RefreshToken,
                                    Permissions.ResponseTypes.Code,
                                    Permissions.Scopes.Email,
                                    Permissions.Scopes.Profile,
                                    Permissions.Scopes.Roles,
                                    Permissions.Prefixes.Scope + Constants.OpenIddictScopes.OutOfSchoolApi,
                                },
                                Requirements =
                                {
                                    Requirements.Features.ProofKeyForCodeExchange,
                                },
                            };
                        }
                        if (descriptor.PostLogoutRedirectUris.Any())
                        {
                            descriptor.PostLogoutRedirectUris.UnionWith(client.PostLogoutRedirectUris.Select(s => new Uri(s)));
                        }
                        if (descriptor.RedirectUris.Any())
                        {
                            descriptor.RedirectUris.UnionWith(client.RedirectUris.Select(s => new Uri(s)));
                        }
                        foreach (var entry in client.DisplayNames)
                        {
                            descriptor.DisplayNames.Add(CultureInfo.GetCultureInfo(entry.Key), entry.Value);
                        }
                    }

                    await manager.CreateAsync(descriptor);
                }
            }
        }

        // TODO: Maybe extract to appsettigns too later.
        static async Task RegisterScopesAsync(IServiceProvider provider)
        {
            var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByNameAsync(Constants.OpenIddictScopes.OutOfSchoolApi) is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "outofschoolapi API access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("uk-UA")] = "Позашкілля",
                        [CultureInfo.GetCultureInfo("en-US")] = "Pozashkillia",
                        [CultureInfo.GetCultureInfo("en-GB")] = "Pozashkillia",
                    },
                    Name = Constants.OpenIddictScopes.OutOfSchoolApi, //"outofschoolapi",
                    Resources =
                    {
                       Constants.OpenIddictResources.OutOfSchoolApi,
                    },
                });
            }
            if (await manager.FindByNameAsync(Constants.OpenIddictScopes.ExternalExportRead) is null)
            {
                await manager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    DisplayName = "external_api API access",
                    DisplayNames =
                    {
                        [CultureInfo.GetCultureInfo("en-US")] = "External API access",
                    },
                    Name = Constants.OpenIddictScopes.ExternalExportRead,
                    Resources =
                    {
                        Constants.OpenIddictResources.OutOfSchoolApi,
                    },
                });
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}