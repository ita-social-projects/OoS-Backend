using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

namespace OutOfSchool.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Read Configuration from appSettings
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            
            // Initialize Logger
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config, sectionName: "Logging")
                .Enrich.FromLogContext()
                .WriteTo.File(
                    path: "log.txt", 
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 2, 
                    fileSizeLimitBytes: null)
                .WriteTo.Debug()
                .WriteTo.Console()
                .CreateLogger();
            
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
                .UseSerilog() // Uses Serilog instead of default .NET Logger
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
