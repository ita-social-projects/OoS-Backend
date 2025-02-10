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
                foreach (var client in options.OpenIdClients)
                {
                    if (await manager.FindByClientIdAsync(client.ClientId) is null)
                    {
                        OpenIddictApplicationDescriptor descriptor;
                        if (client.IsIntrospection)
                        {
                            descriptor = new()
                            {
                                ClientId = client.ClientId,
                                ClientSecret = options.IntrospectionSecret,
                                Permissions =
                                {
                                    Permissions.Endpoints.Introspection,
                                },
                            };
                        }
                        else
                        {
                            descriptor = new OpenIddictApplicationDescriptor
                            {
                                ClientId = client.ClientId,
                                ConsentType = ConsentTypes.Implicit,
                                DisplayName = client.DisplayName,
                                Permissions =
                                {
                                    Permissions.Endpoints.Authorization,
                                    Permissions.Endpoints.Logout,
                                    Permissions.Endpoints.Token,
                                    Permissions.Endpoints.Revocation,
                                    Permissions.GrantTypes.AuthorizationCode,
                                    Permissions.GrantTypes.RefreshToken,
                                    Permissions.ResponseTypes.Code,
                                    Permissions.Scopes.Email,
                                    Permissions.Scopes.Profile,
                                    Permissions.Scopes.Roles,
                                    Permissions.Prefixes.Scope + "outofschoolapi",
                                },
                                Requirements =
                                {
                                    Requirements.Features.ProofKeyForCodeExchange,
                                },
                            };
                            descriptor.PostLogoutRedirectUris.UnionWith(client.PostLogoutRedirectUris.Select(s => new Uri(s)));
                            descriptor.RedirectUris.UnionWith(client.RedirectUris.Select(s => new Uri(s)));
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

                if (await manager.FindByNameAsync("outofschoolapi") is null)
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
                        Name = "outofschoolapi",
                        Resources =
                        {
                            "outofschool_api",
                        },
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }