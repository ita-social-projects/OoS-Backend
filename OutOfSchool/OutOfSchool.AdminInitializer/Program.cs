using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using OutOfSchool.AdminInitializer;
using OutOfSchool.AdminInitializer.Config;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Common.Models;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
        // TODO: Move version check into an extension to reuse code across apps
        var mariaDbServerVersion = config["MariaDbServerVersion"];
        var serverVersion = new MariaDbServerVersion(new Version(mariaDbServerVersion));
        if (serverVersion.Version.Major < Constants.MariaDbServerMinimalMajorVersion)
        {
            throw new InvalidOperationException("MariaDb Server version should be 11 or higher.");
        }

        var connectionString = config.GetMySqlConnectionString<InitializerConnectionOptions>(
            "DefaultConnection",
            options => new MySqlConnectionStringBuilder
            {
                Server = options.Server,
                Port = options.Port,
                UserID = options.UserId,
                Password = options.Password,
                Database = options.Database,
                GuidFormat = options.GuidFormat.ToEnum(MySqlGuidFormat.Default),
                SslMode = options.SslMode.ToEnum(MySqlSslMode.None),
            });

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySql(
                    connectionString,
                    serverVersion,
                    optionsBuilder =>
                        optionsBuilder
                            .EnableStringComparisonTranslations()
                            .UseMicrosoftJson()));
        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<OutOfSchoolDbContext>();
        services.Configure<AdminConfiguration>(config.GetSection(AdminConfiguration.Name));
        services.AddCustomDataProtection("IdentityServer");
        services.AddScoped<AdminInitializer>();
        services.AddTransient<ICurrentUser>(_ => new CurrentUserAccessor(null));
        services.AddTransient<BusinessEntityInterceptor>();
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

var initializer = scope.ServiceProvider.GetRequiredService<AdminInitializer>();

var result = await initializer.InitAdminUser();

Environment.Exit(result);