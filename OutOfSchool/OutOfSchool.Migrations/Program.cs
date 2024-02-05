using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.AuthCommon;
using OutOfSchool.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        var connectionString = config.GetConnectionString("DefaultConnection");

        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySQL(
                    connectionString,
                    optionsBuilder =>
                        optionsBuilder
                            .MigrationsAssembly(migrationsAssembly)))
            .AddDbContext<OpenIdDictDbContext>(options => options
                .UseMySQL(
                    connectionString,
                    optionsBuilder =>
                        optionsBuilder
                            .MigrationsAssembly(migrationsAssembly)));
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<OpenIdDictDbContext>().Database.Migrate();