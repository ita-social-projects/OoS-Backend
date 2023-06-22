using OpenIddict.Abstractions;
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

                // Angular UI client
                if (await manager.FindByClientIdAsync("angular") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "angular",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "angular client PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("uk-UA")] = "Позашкілля",
                            [CultureInfo.GetCultureInfo("en-US")] = "Pozashkillia",
                            [CultureInfo.GetCultureInfo("en-GB")] = "Pozashkillia",
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
                    });
                }

                // API validation
                if (await manager.FindByClientIdAsync("outofschool_api") == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "outofschool_api",
                        ClientSecret = "outofschool_api_secret",
                        Permissions =
                        {
                            Permissions.Endpoints.Introspection,
                        },
                    };

                    await manager.CreateAsync(descriptor);
                }

                // Swagger
                if (await manager.FindByClientIdAsync("Swagger") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "Swagger",
                        Type = ClientTypes.Public,
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "Swagger UI PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("uk-UA")] = "Позашкілля API",
                            [CultureInfo.GetCultureInfo("en-US")] = "Pozashkillia API",
                            [CultureInfo.GetCultureInfo("en-GB")] = "Pozashkillia API",
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("http://localhost:5000/swagger/oauth2-redirect.html"),
                            new Uri("http://localhost:8080/swagger/oauth2-redirect.html"),
                        },
                        RedirectUris =
                        {
                            new Uri("http://localhost:5000/swagger/oauth2-redirect.html"),
                            new Uri("http://localhost:8080/swagger/oauth2-redirect.html"),
                        },
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