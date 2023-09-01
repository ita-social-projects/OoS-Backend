using GrpcServiceServer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Net.Http.Headers;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthorizationServer.Config;
using OutOfSchool.AuthorizationServer.Extensions;
using OutOfSchool.AuthorizationServer.KeyManagement;
using OutOfSchool.AuthorizationServer.Services;
using System.Configuration;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace OutOfSchool.AuthorizationServer;

public static class Startup
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

        // TODO: Move version check into an extension to reuse code across apps
        var mySQLServerVersion = config["MySQLServerVersion"];
        var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
        if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
        {
            throw new Exception("MySQL Server version should be 8 or higher.");
        }

        var quartzConfig = config.GetSection(QuartzConfig.Name).Get<QuartzConfig>();
        services.AddDefaultQuartz(
            config,
            quartzConfig.ConnectionStringKey);

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

        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>()
            .AddDefaultTokenProviders();

        var expireDaysStr = config["CookieConfig:ExpireDays"];
        services.ConfigureApplicationCookie(c =>
        {
            c.Cookie.Name = "OpenIdDict.Cookie";
            c.LoginPath = "/Auth/Login";
            c.LogoutPath = "/Auth/Logout";
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

        // Must be AFTER services.AddIdentity() call
        services
            .AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = Constants.DefaultAuthScheme;
            options.DefaultAuthenticateScheme = Constants.DefaultAuthScheme;
            options.DefaultScheme = Constants.DefaultAuthScheme;
        })
            .AddPolicyScheme(Constants.DefaultAuthScheme, Constants.DefaultAuthScheme, options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                string authorization = context.Request.Headers[HeaderNames.Authorization];
                if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                {
                    return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                }

                return IdentityConstants.ApplicationScheme;
            };
        });

        var authorizationSection = config.GetSection(AuthorizationServerConfig.Name);
        var authorizationConfig = authorizationSection.Get<AuthorizationServerConfig>();
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
                }
                else
                {
                    var certificate = ExternalCertificate.LoadCertificates(authorizationConfig.Certificate);
                    // TODO: create two different certificates after testing this
                    options.AddSigningCertificate(certificate)
                        .AddEncryptionCertificate(certificate);
                }

                options.DisableAccessTokenEncryption(); //TODO: Maybe do encrypt? :)
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

        services.AddHostedService<Worker>(); // TODO: Move to Quartz
        services.AddProxy();
        services.AddAuthCommon(config, builder.Environment.IsDevelopment());
        services.AddTransient<IInteractionService, InteractionService>();
        services.AddTransient<IProfileService, ProfileService>();

        services.AddHealthChecks()
            .AddDbContextCheck<OutOfSchoolDbContext>(
                "Database",
                tags: new[] {"readiness"});
    }

    public static void Configure(this WebApplication app)
    {
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
            .WithMetadata(new AllowAnonymousAttribute());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        app.MapRazorPages();

        var gRPCConfig = app.Configuration.GetSection(GRPCConfig.Name).Get<GRPCConfig>();

        if (gRPCConfig.Enabled)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ProviderAdminServiceGRPC>().RequireHost($"*:{gRPCConfig.Port}");
            });
        }
    }
}