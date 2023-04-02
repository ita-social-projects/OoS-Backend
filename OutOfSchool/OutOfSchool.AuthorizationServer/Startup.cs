using GrpcServiceServer;
using Microsoft.Extensions.FileProviders;
using OpenIddict.Abstractions;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthorizationServer.Config;
using OutOfSchool.AuthorizationServer.Extensions;
using OutOfSchool.AuthorizationServer.KeyManagement;
using OutOfSchool.AuthorizationServer.Services;

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
            quartzConfig.ConnectionStringKey,
            q =>
            {
                // TODO: 
            });

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
                            .MigrationsAssembly(migrationsAssembly))
                .UseOpenIddict());

        services.AddCustomDataProtection("AuthorizationServer");

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        // TODO: remove?
        services.AddAuthentication("Bearer");

        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<CustomClaimsSingInManager>();
        services.ConfigureApplicationCookie(c =>
        {
            c.Cookie.Name = "IdentityServer.Cookie";
            c.LoginPath = "/Auth/Login";
            c.LogoutPath = "/Auth/Logout";
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

        //TODO: fow now do this only for Google

        // if (builder.Environment.IsDevelopment())
        // {
            // services.AddDbContext<CertificateDbContext>(options => options.UseMySql(
            //     connectionString,
            //     serverVersion,
            //     sql => sql.MigrationsAssembly(migrationsAssembly)));

            // services.AddLazyCache();
            // services.AddSingleton<IKeyManager, KeyManager>();
            // services.AddSingleton<IOptionsMonitor<OpenIddictServerOptions>, OpenIdDictServerOptionsProvider>();
            // services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, OpenIdDictServerOptionsInitializer>();
        // }

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<OutOfSchoolDbContext>();
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
                    "outofschoolapi.read",
                    "outofschoolapi.write");

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

                if (builder.Environment.IsEnvironment("Google"))
                {
                    // options.AddSigningKey()
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
                        .WithOrigins(
                            "http://localhost:4200", "http://localhost:5000")
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        services.AddHostedService<Worker>(); // TODO: Move to Quartz
        services.AddProxy();
        services.AddAuthCommon(config, builder.Environment.IsDevelopment());
        services.AddTransient<IInteractionService, InteractionService>();
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

        var commonAssembly = typeof(AuthController).Assembly;
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new ManifestEmbeddedFileProvider(commonAssembly, "wwwroot"),
            RequestPath = string.Empty,
        });

        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        var gRPCConfig = app.Configuration.GetSection(GRPCConfig.Name).Get<GRPCConfig>();

        if (gRPCConfig.Enabled)
        {
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ProviderAdminServiceGRPC>().RequireHost($"*:{gRPCConfig.Port}");
            });
        }
    }

    public static void RolesInit(RoleManager<IdentityRole> manager)
    {
        var roles = new IdentityRole[]
        {
            new IdentityRole {Name = "parent"},
            new IdentityRole {Name = "provider"},
            new IdentityRole {Name = "techadmin"},
            new IdentityRole {Name = "ministryadmin"},
        };
        foreach (var role in roles)
        {
            manager.CreateAsync(role).Wait();
        }
    }
}