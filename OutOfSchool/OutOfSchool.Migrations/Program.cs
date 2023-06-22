using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using OutOfSchool.AuthorizationServer;
using OutOfSchool.Common;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.IdentityServer.Extensions;
using OutOfSchool.IdentityServer.KeyManagement;
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
            throw new Exception("MySQL Server version should be 8 or higher.");
        }

        var connectionString = config.GetMySqlConnectionString<MySqlGuidConnectionOptions>(
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

        var migrationsAssembly = typeof(Program).GetTypeInfo().Assembly.GetName().Name;

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .MigrationsAssembly(migrationsAssembly)))
            .AddDbContext<OpenIdDictDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .MigrationsAssembly(migrationsAssembly)));

        services.ConfigureIdentity(connectionString, config["Uri"], serverVersion, migrationsAssembly);
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<CertificateDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<OutOfSchoolDbContext>().Database.Migrate();
scope.ServiceProvider.GetRequiredService<OpenIdDictDbContext>().Database.Migrate();