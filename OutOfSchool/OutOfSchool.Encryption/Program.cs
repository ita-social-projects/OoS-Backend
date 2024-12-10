using Asp.Versioning;
using Elastic.Apm.DiagnosticSource;
using OutOfSchool.Encryption.Config;
using OutOfSchool.Encryption.Constants;
using OutOfSchool.Encryption.Handlers;
using OutOfSchool.Encryption.Services;
using Serilog;
using Serilog.Context;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.jsonc", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.jsonc", true, true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()));

GlobalLogContext.PushProperty("AppVersion", builder.Configuration.GetSection("AppDefaults:Version").Value);

builder.Services.AddSingleton(Log.Logger);

builder.Services.AddElasticApmForAspNetCore(new HttpDiagnosticsSubscriber());
builder.Services
    .AddOptions<EUSignConfig>()
    .BindConfiguration(EUSignConfig.ConfigSectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEUSignOAuth2Service, DevEUSignOAuth2Service>();
}
else
{
    builder.Services.AddSingleton<IIoOperationsService, IoOperationsService>();
    builder.Services.AddSingleton<IEUSignOAuth2Service, EUSignOAuth2Service>();
}

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(AppConstants.ApiVersion1);
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

var app = builder.Build();

app.UseSerilogRequestLogging();

var apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(AppConstants.ApiVersion1))
    .ReportApiVersions()
    .Build();

app.UseExceptionHandler(handler => handler.Run(async ctx => await Results.Problem().ExecuteAsync(ctx)));

app.MapAppHandlers(apiVersionSet);

try
{
    using (var scope = app.Services.CreateScope())
    {
        // Check EUSign is able to initialize
        // There's no reason to run this application without it.
        scope.ServiceProvider.GetRequiredService(typeof(IEUSignOAuth2Service));
    }

    Log.Information("Application has started");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}