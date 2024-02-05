using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using OutOfSchool.AdminInitializer;
using OutOfSchool.AdminInitializer.Config;
using OutOfSchool.Common.Extensions.Startup;
using OutOfSchool.Services;
using OutOfSchool.Services.Extensions;
using OutOfSchool.Services.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        var connectionString = config.GetMySqlConnectionString<InitializerConnectionOptions>(
            "DefaultConnection",
            options => new MySqlConnectionStringBuilder
            {
                Server = options.Server,
                Port = options.Port,
                UserID = options.UserId,
                Password = options.Password,
                Database = options.Database,
                OldGuids = options.OldGuids,
            });

        services
            .AddDbContext<OutOfSchoolDbContext>(options => options
                .UseMySQL(
                    connectionString,
                    optionsBuilder =>
                        optionsBuilder
                            .EnableRetryOnFailure(
                                3,
                                TimeSpan.FromSeconds(5),
                                null)));
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
    })
    .Build();

using var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

var initializer = scope.ServiceProvider.GetRequiredService<AdminInitializer>();

var result = await initializer.InitAdminUser();

Environment.Exit(result);