using System;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OutOfSchool.WebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            const string outputTemplate = "{ApplicationName} | {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u4}] | {Message:l}{NewLine}{Exception}";
           
            var config = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.{environment}.json")
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

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}