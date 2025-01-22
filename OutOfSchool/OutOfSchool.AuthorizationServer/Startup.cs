using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.EntityFrameworkCore;
using GrpcServiceServer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.JsonWebTokens;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthCommon.Validators;
using OutOfSchool.AuthorizationServer.Config;
using OutOfSchool.AuthorizationServer.Extensions;
using OutOfSchool.AuthorizationServer.External;
using OutOfSchool.AuthorizationServer.KeyManagement;
using OutOfSchool.AuthorizationServer.Services;
using OutOfSchool.Common.Validators;
using OutOfSchool.EmailSender.Services;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace OutOfSchool.AuthorizationServer;

public static class Startup
{
    public static async Task AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        services.AddElasticApmForAspNetCore(
            new HttpDiagnosticsSubscriber(),
            new EfCoreDiagnosticsSubscriber());

        var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        // TODO: Move version check into an extension to reuse code across apps
        var mySQLServerVersion = config["MySQLServerVersion"];
        var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
        if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
        {
            throw new InvalidOperationException("MySQL Server version should be 8 or higher.");
        }

        var quartzConfig = config.GetSection(QuartzConfig.Name).Get<QuartzConfig>();
        await services.AddDefaultQuartz(
            config,
            quartzConfig.ConnectionStringKey,
            t => t.AddEmailSender(quartzConfig));

        var connectionString = config.GetMySqlConnectionString<AuthorizationConnectionOptions>(
            "DefaultConnection",
            options => new MySqlConnectionStringBuilder
            {
                Server = options.Server,
                Port = options.Port,
                UserID = options.UserId,
                Password = options.Password,
                Database = options.Database,
                GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
            });

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .UseMicrosoftJson()
                            .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)
                            .MigrationsAssembly(migrationsAssembly)))
            .AddDbContext<OpenIdDictDbContext>(options => options
            .UseMySql(
                connectionString,
                serverVersion,
                optionsBuilder =>
                    optionsBuilder
                        .MigrationsAssembly(migrationsAssembly)));

        services.AddCustomDataProtection("AuthorizationServer");

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddTransient<ICustomPasswordRules, CustomPasswordRules>();
        services.AddTransient<IPasswordValidator<User>, CustomPasswordValidator>();

        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.User.AllowedUserNameCharacters = AuthServerConstants.AllowedUserNameCharacters;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>()
            .AddDefaultTokenProviders();

        var expireDaysStr = config["CookieConfig:ExpireDays"];
        services.ConfigureApplicationCookie(c =>
        {
            c.Cookie.Name = "OpenIdDict.Cookie";
            c.LoginPath = $"/{AuthServerConstants.LoginPath}";
            c.LogoutPath = $"/{AuthServerConstants.LogoutPath}";
            c.ExpireTimeSpan = TimeSpan.FromDays(Convert.ToInt32(expireDaysStr));
        });

        var issuerSection = config.GetSection(IssuerConfig.Name);
        services.Configure<IssuerConfig>(issuerSection);

        services.Configure<IdentityOptions>(options =>
        {
            // Configure Identity to use the same JWT claims as OpenIddict instead
            // of the legacy WS-Federation claims it uses by default (ClaimTypes),
            // which saves you from doing the mapping in your authorization controller.
            options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
            options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
            options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            options.ClaimsIdentity.EmailClaimType = OpenIddictConstants.Claims.Email;

            // Note: to require account confirmation before login,
            // register an email sender service (IEmailSender) and
            // set options.SignIn.RequireConfirmedAccount to true.
            //
            // For more information, visit https://aka.ms/aspaccountconf.
            options.SignIn.RequireConfirmedAccount = false;
        });

        var authorizationSection = config.GetSection(AuthorizationServerConfig.Name);
        var authorizationConfig = authorizationSection.Get<AuthorizationServerConfig>();
        ConfigurationValidationHelper.ValidateConfigurationObject(authorizationConfig);
        services.Configure<AuthorizationServerConfig>(authorizationSection);
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<OpenIdDictDbContext>();
                options.UseQuartz();
            })
            .AddServer(options =>
            {
                // Enable the authorization, logout, token and userinfo endpoints.
                options.SetAuthorizationEndpointUris("connect/authorize")
                    //.SetDeviceEndpointUris("connect/device")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetLogoutEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserinfoEndpointUris("connect/userinfo")
                    .SetVerificationEndpointUris("connect/verify");

                options.AllowAuthorizationCodeFlow()
                    .AllowHybridFlow()
                    .AllowClientCredentialsFlow()
                    .AllowRefreshTokenFlow();

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles,
                    "outofschoolapi");

                var aspNetCoreBuilder = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                if (builder.Environment.IsDevelopment())
                {
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                    aspNetCoreBuilder.DisableTransportSecurityRequirement();
                    options.DisableAccessTokenEncryption();
                }
                else
                {
                    var certificate = ExternalCertificate.LoadCertificates(authorizationConfig.Certificate);
                    // TODO: create two different certificates after testing this
                    options.AddSigningCertificate(certificate)
                        .AddEncryptionCertificate(certificate);
                }
            })
            .AddClient(options =>
            {
                options.AddRegistration(new OpenIddictClientRegistration
                {
                    Issuer = authorizationConfig.ExternalLogin.IdServerUri,
                    ClientId = authorizationConfig.ExternalLogin.ClientId,
                    ClientSecret = authorizationConfig.ExternalLogin.ClientSecret,
                    RedirectUri = new Uri($"{config["Identity:Authority"]}/callback/idgovua"),
                    ProviderName = "IdGovUa",
                    ProviderDisplayName = "id.gov.ua",
                    Scopes = { OpenIddictConstants.Scopes.Profile },

                    // Token validation is not supported by id.gov.ua.
                    TokenValidationParameters =
                    {
                        SignatureValidator = (token, _) =>
                        {
                            var handler = new JsonWebTokenHandler();
                            return handler.ReadJsonWebToken(token);
                        },
                    },
                    Configuration = new OpenIddictConfiguration
                    {
                        Issuer = authorizationConfig.ExternalLogin.IdServerUri,
                        AuthorizationEndpoint = new Uri(authorizationConfig.ExternalLogin.IdServerUri, authorizationConfig.ExternalLogin.IdServerPaths.Authorize),
                        TokenEndpoint = new Uri(authorizationConfig.ExternalLogin.IdServerUri, authorizationConfig.ExternalLogin.IdServerPaths.Token),
                        ResponseTypesSupported = { OpenIddictConstants.ResponseTypes.Code },
                        TokenEndpointAuthMethodsSupported = {OpenIddictConstants.ClientAuthenticationMethods.ClientSecretPost},
                        UserinfoEndpoint = null,
                    },
                });
                options.UseSystemNetHttp();
                
                options
                    .AllowClientCredentialsFlow()
                    .AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .AddEventHandler(ExtractUserIdFromTokenResponseHandler.Descriptor);

                var aspNetCoreBuilder = options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough()
                    .EnablePostLogoutRedirectionEndpointPassthrough();

                if (builder.Environment.IsDevelopment())
                {
                    options.AddDevelopmentSigningCertificate()
                        .AddEphemeralEncryptionKey();
                    aspNetCoreBuilder.DisableTransportSecurityRequirement();
                }
                else
                {
                    var certificate = ExternalCertificate.LoadCertificates(authorizationConfig.Certificate);
                    // TODO: create two different certificates after testing this
                    options.AddSigningCertificate(certificate)
                        .AddEncryptionCertificate(certificate);
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddCors(options =>
        {
            options.AddPolicy(
                "AllowAllOrigins",
                policyBuilder =>
                {
                    policyBuilder
                        .AllowCredentials()
                        .WithOrigins(authorizationConfig.AllowedCorsOrigins)
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        services.AddHttpClient(config["Communication:ClientName"])
            .ConfigurePrimaryHttpMessageHandler(handler =>
                new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip,
                });

        services.Configure<CommunicationConfig>(config.GetSection(CommunicationConfig.Name));
        services.AddHostedService<Worker>(); // TODO: Move to Quartz
        services.AddProxy();
        services.AddTransient<IGovIdentityCommunicationService, GovIdentityCommunicationService>();
        services.AddAuthCommon(config, builder.Environment.IsDevelopment());
        services.AddTransient<IInteractionService, InteractionService>();
        services.AddTransient<IProfileService, ProfileService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<ISendGridAccessibilityService, SendGridAccessibilityService>();

        services.AddHostedService<IdentityRolesInitializerHostedService>();

        services.AddHealthChecks()
            .AddDbContextCheck<OutOfSchoolDbContext>(
                "Database",
                tags: new[] {"readiness"});
    }

    public static void Configure(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var httpRequest = context.Request;
            var httpResponse = context.Response;

            bool healthCheck = httpRequest.Path.Equals("/healthz/ready");

            int healthPort = app.Configuration.GetValue<int>("ApplicationPorts:HealthPort");

            if (httpRequest.HttpContext.Connection.LocalPort == healthPort && !healthCheck)
            {
                httpResponse.StatusCode = 404;
                return;
            }

            await next();
        });

        var proxyOptions = app.Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
        app.UseProxy(proxyOptions);
        app.UseCors("AllowAllOrigins");

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        var supportedCultures = new[]
        {
            new CultureInfo("en"),
            new CultureInfo("uk"),
        };

        var requestLocalization = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture("uk"),
            SupportedCultures = supportedCultures,
            SupportedUICultures = supportedCultures,
            RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new CustomRequestCultureProvider(context =>
                {
                    if (!context.Request.Query.TryGetValue("ui-culture", out var selectedUiCulture) &
                        !context.Request.Query.TryGetValue("culture", out var selectedCulture))
                    {
                        var encodedPathAndQuery = context.Request.GetEncodedPathAndQuery();
                        var decodedUrl = WebUtility.UrlDecode(encodedPathAndQuery);
                        var dictionary = QueryHelpers.ParseQuery(decodedUrl);
                        dictionary.TryGetValue("ui-culture", out selectedUiCulture);
                        dictionary.TryGetValue("culture", out selectedCulture);
                    }

                    if (selectedCulture.FirstOrDefault() is null ^ selectedUiCulture.FirstOrDefault() is null)
                    {
                        return Task.FromResult(new ProviderCultureResult(
                            selectedCulture.FirstOrDefault() ??
                            selectedUiCulture.FirstOrDefault()));
                    }

                    return Task.FromResult(new ProviderCultureResult(
                        selectedCulture.FirstOrDefault(),
                        selectedUiCulture.FirstOrDefault()));
                }),
            },
        };

        app.UseRequestLocalization(requestLocalization);

        app.UseRouting();

        app.UseCookiePolicy(new CookiePolicyOptions {MinimumSameSitePolicy = SameSiteMode.Lax});

        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
                AllowCachingResponses = false,
            })
            .RequireHost($"*:{app.Configuration.GetValue<int>("ApplicationPorts:HealthPort")}")
            .WithMetadata(new AllowAnonymousAttribute());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        app.MapRazorPages();

        var gRPCConfig = app.Configuration.GetSection(GrpcConfig.Name).Get<GrpcConfig>();

        if (gRPCConfig.Enabled)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ProviderAdminServiceGrpc>().RequireHost($"*:{gRPCConfig.Port}");
            });
        }
    }
}