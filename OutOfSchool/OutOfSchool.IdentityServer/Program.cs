using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.IdentityServer.Config;
using OutOfSchool.IdentityServer.KeyManagement;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using Serilog;
using Serilog.Formatting.Compact;

namespace OutOfSchool.IdentityServer
{
    public class Program
    {
        private const int CheckConnectivityDelay = 5000;

        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{environment}.json")
                .AddEnvironmentVariables()
                .Build();

            var loggerConfigBuilder = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Logging")
                .Enrich.FromLogContext()
                .WriteTo.Debug();

            if (environment != "Azure" && environment != "Google")
            {
                loggerConfigBuilder
                    .WriteTo.Console()
                    .WriteTo.File(
                    path: config.GetSection("Logging:FilePath").Value,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 2,
                    fileSizeLimitBytes: null);
            }
            else
            {
                loggerConfigBuilder.WriteTo.Console(new RenderedCompactJsonFormatter());
            }

            Log.Logger = loggerConfigBuilder.CreateLogger();

            try
            {
                Log.Information("Application has started.");

                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                    while (!context.Database.CanConnect())
                    {
                        Log.Information("Waiting for db connection");
                        Task.Delay(CheckConnectivityDelay).Wait();
                    }

                    scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>()
                        .Database.Migrate();
                    scope.ServiceProvider.GetRequiredService<CertificateDbContext>()
                        .Database.Migrate();
                    var identityContext = scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>();
                    var configService = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                    // TODO: Move to identity options
                    var apiSecret = configService["outofschoolapi:ApiSecret"];
                    var clientSecret = configService["m2m.client:ClientSecret"];
                    var identityOptions = new IdentityAccessOptions();
                    configService.GetSection(identityOptions.Name).Bind(identityOptions);

                    context.Database.Migrate();
                    identityContext.Database.Migrate();
                    var manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    if (!manager.Roles.Any())
                    {
                        RolesInit(manager);
                    }

                    foreach (var client in StaticConfig.Clients(clientSecret, identityOptions.AdditionalIdentityClients))
                    {
                        context.Clients.AddIfNotExists(client.ToEntity(), c => c.ClientId == client.ClientId);
                    }

                    context.SaveChanges();

                    foreach (var resource in StaticConfig.IdentityResources)
                    {
                        context.IdentityResources.AddIfNotExists(resource.ToEntity(), ir => resource.Name == ir.Name);
                    }

                    context.SaveChanges();

                    foreach (var resource in StaticConfig.ApiResources(apiSecret))
                    {
                        context.ApiResources.AddIfNotExists(resource.ToEntity(), ar => resource.Name == ar.Name);
                    }

                    context.SaveChanges();

                    foreach (var resource in StaticConfig.ApiScopes)
                    {
                        context.ApiScopes.AddIfNotExists(resource.ToEntity(), apiScope => apiScope.Name == resource.Name);
                    }

                    context.SaveChanges();
                }

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void RolesInit(RoleManager<IdentityRole> manager)
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