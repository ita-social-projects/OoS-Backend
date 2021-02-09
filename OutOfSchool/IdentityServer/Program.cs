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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OutOfSchool.IdentityServer;
using OutOfSchool.IdentityServer.Data;
using OutOfSchool.Services;

namespace OutOfSchool.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                // TODO: Move from Console to logger
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

        private static void RolesInit(RoleManager<IdentityRole> manager)
        {
            var roles = new IdentityRole[]
            {
                new IdentityRole {Name = "parent"},
                new IdentityRole {Name = "organization"},
                new IdentityRole {Name = "admin"}
            };
            foreach (var role in roles)
            {
                manager.CreateAsync(role).Wait();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
