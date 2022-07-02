using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.IdentityServer;
using OutOfSchool.IdentityServer.Config;
using OutOfSchool.IdentityServer.KeyManagement;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

GlobalLogContext.PushProperty("AppVersion", builder.Configuration.GetSection("AppDefaults:Version").Value);

// Add services to the container.
builder.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Configure();

try
{
    Log.Information("Application has started.");

    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

    while (!context.Database.CanConnect())
    {
        Log.Information("Connection to db");
        await Task.Delay(app.Configuration.GetValue<int>("CheckConnectivityDelay"));
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
        Startup.RolesInit(manager);
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

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
