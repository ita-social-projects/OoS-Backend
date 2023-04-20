using Google.Api;
using Google.Cloud.Logging.V2;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;

namespace OutOfSchool.IdentityServer.Services;

public class AdditionalClientsHostedService : IHostedService
{
    private readonly IServiceProvider serviceProvider;

    public AdditionalClientsHostedService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

        var configService = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        while (!context.Database.CanConnect())
        {
            Log.Information("Connection to db");
            await Task.Delay(configService.GetValue<int>("CheckConnectivityDelay"));
        }

        var apiSecret = configService["outofschoolapi:ApiSecret"];
        var clientSecret = configService["m2m.client:ClientSecret"];
        var identityOptions = new IdentityAccessOptions();

        configService.GetSection(identityOptions.Name).Bind(identityOptions);

        var manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        IdentityRolesCreator.Create(manager);

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

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
