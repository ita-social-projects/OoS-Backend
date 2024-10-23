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
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.jsonc", true, true);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()));

GlobalLogContext.PushProperty("AppVersion", builder.Configuration.GetSection("AppDefaults:Version").Value);

builder.Services.AddElasticApmForAspNetCore(new HttpDiagnosticsSubscriber());
builder.Services.Configure<EUSignConfig>(
    builder.Configuration.GetSection(EUSignConfig.ConfigSectionName));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IEUSignOAuth2Service, DevEUSignOAuth2Service>();
}
else
{
    builder.Services.AddSingleton<IEUSignOAuth2Service, EUSignOAuth2Service>();
}

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
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
    .HasApiVersion(new ApiVersion(AppConstants.API_V1))
    .ReportApiVersions()
    .Build();

app.UseExceptionHandler(handler => handler.Run(async ctx => await Results.Problem().ExecuteAsync(ctx)));

app.MapAppHandlers(apiVersionSet);

try
{
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