using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Net.Http.Headers;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.IdentityServer.Extensions;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace OutOfSchool.IdentityServer;

public static class Startup
{
    public static void AddApplicationServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        var migrationsAssembly = config["MigrationsAssembly"];

        // TODO: Move version check into an extension to reuse code across apps
        var mySQLServerVersion = config["MySQLServerVersion"];
        var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
        if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
        {
            throw new Exception("MySQL Server version should be 8 or higher.");
        }

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
                            .MigrationsAssembly(migrationsAssembly)));

        services.AddCustomDataProtection("IdentityServer");

        var issuerSection = config.GetSection(IssuerConfig.Name);
        services.Configure<IssuerConfig>(issuerSection);

        services.ConfigureIdentity(
            connectionString,
            issuerSection["Uri"],
            serverVersion,
            migrationsAssembly,
            identityBuilder =>
            {
                identityBuilder.AddTokenProvider<DataProtectorTokenProvider<User>>(TokenOptions.DefaultProvider);
            },
            identityServerBuilder =>
            {
                identityServerBuilder.AddAspNetIdentity<User>();
                identityServerBuilder.AddProfileService<ProfileService>();
            });

        // Must be AFTER services.AddIdentity() call
        services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = Constants.DefaultAuthScheme;
                options.DefaultAuthenticateScheme = Constants.DefaultAuthScheme;
                options.DefaultScheme = Constants.DefaultAuthScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = config["Identity:Authority"];
                options.Audience = config["Identity:ApiName"];
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters.ValidAudiences = new[] {config["Identity:ApiName"]};
                options.SaveToken = true;
                options.MapInboundClaims = false;
            })
            .AddPolicyScheme(Constants.DefaultAuthScheme, Constants.DefaultAuthScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    string authorization = context.Request.Headers[HeaderNames.Authorization];
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    return IdentityConstants.ApplicationScheme;
                };
            });

        services.ConfigureApplicationCookie(c =>
        {
            c.Cookie.Name = "IdentityServer.Cookie";
            c.LoginPath = "/Auth/Login";
            c.LogoutPath = "/Auth/Logout";
        });
        services.AddProxy();
        services.AddAuthCommon(config, builder.Environment.IsDevelopment());
        services.AddTransient<IInteractionService, InteractionService>();

        services.AddHostedService<AdditionalClientsHostedService>();

        services.AddHealthChecks()
            .AddDbContextCheck<OutOfSchoolDbContext>(
                "Database",
                tags: new[] {"readiness"});
    }

    public static void Configure(this WebApplication app)
    {
        var proxyOptions = app.Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
        app.UseProxy(proxyOptions);

        app.UseSecurityHttpHeaders(app.Environment.IsDevelopment());

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

        app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

        app.UseStaticFiles();

        app.UseSerilogRequestLogging();

        app.UseAuthentication();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("readiness"),
                AllowCachingResponses = false,
            })
            .WithMetadata(new AllowAnonymousAttribute());

        app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });
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
