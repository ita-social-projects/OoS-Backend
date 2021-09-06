using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OutOfSchool.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{environment}.json")
                .AddEnvironmentVariables()
                .Build();

            var loggerConfigBuilder = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Logging")
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.Console();

            if (environment != "Azure")
            {
                loggerConfigBuilder.WriteTo.File(
                    path: config.GetSection("Logging:FilePath").Value,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 2,
                    fileSizeLimitBytes: null);
            }

            Log.Logger = loggerConfigBuilder.CreateLogger();

            try
            {
                Log.Information("Application has started.");
                CreateHostBuilder(args).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}