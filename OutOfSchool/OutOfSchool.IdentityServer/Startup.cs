using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using GrpcServiceServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using OutOfSchool.Common;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Config;
using OutOfSchool.IdentityServer.KeyManagement;
using OutOfSchool.IdentityServer.Services;
using OutOfSchool.IdentityServer.Services.Intefaces;
using OutOfSchool.IdentityServer.Services.Interfaces;
using OutOfSchool.IdentityServer.Util;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using Serilog;

namespace OutOfSchool.IdentityServer
{
    public static class Startup
    {
        public static void AddApplicationServices(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var config = builder.Configuration;

            services.Configure<IdentityServerConfig>(config.GetSection(IdentityServerConfig.Name));
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            // TODO: Move version check into an extension to reuse code across apps
            var mySQLServerVersion = config["MySQLServerVersion"];
            var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
            if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
            {
                throw new Exception("MySQL Server version should be 8 or higher.");
            }

            var connectionString = config.GetMySqlConnectionString<IdentityConnectionOptions>(
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
                                .MigrationsAssembly("OutOfSchool.IdentityServer")));

            services.AddCustomDataProtection("IdentityServer");

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.ApiName = "outofschoolapi";
                    options.Authority = config["Identity:Authority"];

                    options.RequireHttpsMetadata = false;
                });

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
            services.ConfigureApplicationCookie(c =>
            {
                c.Cookie.Name = "IdentityServer.Cookie";
                c.LoginPath = "/Auth/Login";
                c.LogoutPath = "/Auth/Logout";
            });

            var issuerSection = config.GetSection(IssuerConfig.Name);
            services.Configure<IssuerConfig>(issuerSection);

            // GRPC options
            services.Configure<GRPCConfig>(config.GetSection(GRPCConfig.Name));

            services.AddIdentityServer(options => { options.IssuerUri = issuerSection["Uri"]; })
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseMySql(
                            connectionString,
                            serverVersion,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseMySql(
                            connectionString,
                            serverVersion,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 3600;
                })
                .AddAspNetIdentity<User>()
                .AddProfileService<ProfileService>()
                .AddCustomKeyManagement<CertificateDbContext>(builder =>
                    builder.UseMySql(
                        connectionString,
                        serverVersion,
                        sql => sql.MigrationsAssembly(migrationsAssembly)));

            var mailConfig = config
                .GetSection(EmailOptions.SectionName)
                .Get<EmailOptions>();
            services.AddEmailSender(
                builder.Environment.IsDevelopment(),
                mailConfig.SendGridKey,
                builder => builder.Bind(config.GetSection(EmailOptions.SectionName)));

            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedResource));
                });
            services.AddProxy();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddTransient<IParentRepository, ParentRepository>();
            services.AddTransient<IEntityRepository<PermissionsForRole>, EntityRepository<PermissionsForRole>>();
            services.AddTransient<IProviderAdminRepository, ProviderAdminRepository>();
            services.AddTransient<IProviderAdminService, ProviderAdminService>();

            services.AddTransient<IEntityRepository<ProviderAdminChangesLog>, EntityRepository<ProviderAdminChangesLog>>();
            services.AddTransient<IProviderAdminChangesLogService, ProviderAdminChangesLogService>();

            // Register the Permission policy handlers
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            services.AddGrpc();
        }

        public static void Configure(this WebApplication app)
        {
            var proxyOptions = app.Configuration.GetSection(ReverseProxyOptions.Name).Get<ReverseProxyOptions>();
            app.UseProxy(proxyOptions);

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

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapDefaultControllerRoute(); });

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
                new IdentityRole {Name = "admin"},
            };
            foreach (var role in roles)
            {
                manager.CreateAsync(role).Wait();
            }
        }
    }
}