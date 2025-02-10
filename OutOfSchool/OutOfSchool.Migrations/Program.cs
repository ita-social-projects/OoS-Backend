using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OutOfSchool.AuthCommon;
using OutOfSchool.Common;
using OutOfSchool.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        // TODO: Move version check into an extension to reuse code across apps
        var mySQLServerVersion = config["MySQLServerVersion"];
        var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
        if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
        {
            throw new InvalidOperationException("MySQL Server version should be 8 or higher.");
        }

        var connectionString = config.GetConnectionString("DefaultConnection");

        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .UseMicrosoftJson()
                            .MigrationsAssembly(migrationsAssembly)))
            .AddDbContext<OpenIdDictDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .MigrationsAssembly(migrationsAssembly)));
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<OpenIdDictDbContext>().Database.Migrate();