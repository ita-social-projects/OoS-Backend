using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Common;
using OutOfSchool.Services;
using OutOfSchool.FakeDataSeeder.Services;

namespace OutOfSchool.FakeDataSeeder
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder => builder.AddUserSecrets<Program>())
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context, services);
                })
                .Build();                

            // TODO: Remove it to host
            var seedingService = builder.Services.GetRequiredService<ISeedingService>();
            seedingService.FillWithPredefinedData();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var connectionStringBuilder = new DbConnectionStringBuilder();
            connectionStringBuilder.ConnectionString = connectionString;
            if (!connectionStringBuilder.ContainsKey("guidformat") ||
                connectionStringBuilder["guidformat"].ToString().ToLower() != "binary16")
            {
                throw new Exception(
                    "The connection string should have a key: \"guidformat\" and a value: \"binary16\"");
            }

            var mySQLServerVersion = configuration["MySQLServerVersion"];
            var serverVersion = new MySqlServerVersion(new Version(mySQLServerVersion));
            if (serverVersion.Version.Major < Constants.MySQLServerMinimalMajorVersion)
            {
                throw new Exception("MySQL Server version should be 8 or higher.");
            }

            services
                .AddDbContext<OutOfSchoolDbContext>(options => options
                    .UseMySql(
                        connectionString,
                        serverVersion,
                        optionsBuilder =>
                            optionsBuilder
                                .EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null)));

            services.AddScoped<ISeedingService, SeedingService>();
        }
    }
}
