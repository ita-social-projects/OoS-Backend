using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.Services;
using Serilog;

namespace OutOfSchool.IdentityServer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            const string outputTemplate = "{ApplicationName} | {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] | {Message:l}{NewLine}{Exception}";
         
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{environment}.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Logging")
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", $"{Assembly.GetCallingAssembly().GetName().Name}")
                .WriteTo.File(
                    path: config.GetSection("Logging:FilePath").Value,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 2,
                    fileSizeLimitBytes: null,
                    outputTemplate: outputTemplate,
                    shared: true)
                .WriteTo.Debug()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Application has started.");
                var host = CreateHostBuilder(args).Build();
                
                using (var scope = host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                    while (!context.Database.CanConnect())
                    {
                        Task.Delay(500).Wait();
                        Console.WriteLine("Waiting for db connection");
                    }

                    scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>()
                        .Database.Migrate();
                    var identityContext = scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>();
                    var configService = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var apiSecret = configService["outofschoolapi:ApiSecret"];
                    var clientSecret = configService["m2m.client:ClientSecret"];

                    context.Database.Migrate();
                    identityContext.Database.Migrate();
                    var manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                    if (!manager.Roles.Any())
                    {
                        RolesInit(manager);
                    }

                    if (!context.Clients.Any())
                    {
                        foreach (var client in Config.Clients(clientSecret))
                        {
                            context.Clients.Add(client.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.IdentityResources.Any())
                    {
                        foreach (var resource in Config.IdentityResources)
                        {
                            context.IdentityResources.Add(resource.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.ApiResources.Any())
                    {
                        foreach (var resource in Config.ApiResources(apiSecret))
                        {
                            context.ApiResources.Add(resource.ToEntity());
                        }

                        context.SaveChanges();
                    }

                    if (!context.ApiScopes.Any())
                    {
                        foreach (var resource in Config.ApiScopes)
                        {
                            context.ApiScopes.Add(resource.ToEntity());
                        }

                        context.SaveChanges();
                    }
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

        private static void RolesInit(RoleManager<IdentityRole> manager)
        {
            var roles = new[]
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