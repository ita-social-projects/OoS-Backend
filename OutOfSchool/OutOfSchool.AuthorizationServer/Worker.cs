using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OutOfSchool.AuthorizationServer;

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

                // Angular UI client
                if (await manager.FindByClientIdAsync("angular") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "angular",
                        ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                        DisplayName = "angular client PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("uk-UA")] = "Позашкілля",
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:4200")
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:4200")
                        },
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Logout,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Revocation,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.ResponseTypes.Code,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "outofschoolapi.read",
                            OpenIddictConstants.Permissions.Prefixes.Scope + "outofschoolapi.write",
                        },
                        Requirements =
                        {
                            OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                // // API application CC
                // if (await manager.FindByClientIdAsync("Swagger") == null)
                // {
                //     await manager.CreateAsync(new OpenIddictApplicationDescriptor
                //     {
                //         ClientId = "CC",
                //         ClientSecret = "cc_secret",
                //         DisplayName = "CC for protected API",
                //         Permissions =
                //     {
                //         Permissions.Endpoints.Authorization,
                //         Permissions.Endpoints.Token,
                //         Permissions.GrantTypes.ClientCredentials,
                //         Permissions.Prefixes.Scope + "dataEventRecords"
                //     }
                //     });
                // }

                // // API
                // if (await manager.FindByClientIdAsync("rs_dataEventRecordsApi") == null)
                // {
                //     var descriptor = new OpenIddictApplicationDescriptor
                //     {
                //         ClientId = "rs_dataEventRecordsApi",
                //         ClientSecret = "dataEventRecordsSecret",
                //         Permissions =
                //         {
                //             Permissions.Endpoints.Introspection
                //         }
                //     };
                //
                //     await manager.CreateAsync(descriptor);
                // }

                // Blazor Hosted
                if (await manager.FindByClientIdAsync("Swagger") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "Swagger",
                        Type = ClientTypes.Public,
                        ConsentType = OpenIddictConstants.ConsentTypes.Explicit,
                        DisplayName = "Blazor code PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("uk-UA")] = "Позашкілля API",
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:5000/swagger/oauth2-redirect.html")
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:5000/swagger/oauth2-redirect.html"),
                        },
                        // ClientSecret = "codeflow_pkce_client_secret",
                        Permissions =
                        {
                            OpenIddictConstants.Permissions.Endpoints.Authorization,
                            OpenIddictConstants.Permissions.Endpoints.Logout,
                            OpenIddictConstants.Permissions.Endpoints.Token,
                            OpenIddictConstants.Permissions.Endpoints.Revocation,
                            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                            OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                            OpenIddictConstants.Permissions.ResponseTypes.Code,
                            OpenIddictConstants.Permissions.Scopes.Email,
                            OpenIddictConstants.Permissions.Scopes.Profile,
                            OpenIddictConstants.Permissions.Scopes.Roles,
                            OpenIddictConstants.Permissions.Prefixes.Scope + "outofschoolapi.read",
                            OpenIddictConstants.Permissions.Prefixes.Scope + "outofschoolapi.write",
                        },
                        Requirements =
                        {
                            OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }
            }

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
                        },
                        Name = "outofschoolapi",
                        Resources =
                        {
                            "outofschoolapi"
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }