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

            var isNotGoogle = environment != "Google";
            if (isNotGoogle)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.{environment}.json")
                    .AddEnvironmentVariables()
                    .Build();

                var loggerConfigBuilder = new LoggerConfiguration()
                    .ReadFrom.Configuration(config, sectionName: "Logging")
                    .Enrich.FromLogContext()
                    .WriteTo.Debug();


                loggerConfigBuilder
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: config.GetSection("Logging:FilePath").Value,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 2,
                        fileSizeLimitBytes: null);

                Log.Logger = loggerConfigBuilder.CreateLogger();
            }

            try
            {
                Log.Information("Application has started.");
                CreateHostBuilder(args, isNotGoogle).Build().Run();
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

        public static IHostBuilder CreateHostBuilder(string[] args, bool isNotGoogle)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args);
            if (isNotGoogle)
            {
                hostBuilder.UseSerilog();
            }

            return hostBuilder.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}