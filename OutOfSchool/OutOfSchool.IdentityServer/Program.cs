using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
        .WithDefaultDestructurers()
        .WithDestructurers(new[] { new DbUpdateExceptionDestructurer() })));

GlobalLogContext.PushProperty("AppVersion", builder.Configuration.GetSection("AppDefaults:Version").Value);

// Add services to the container.
builder.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Configure();

try
{
    Log.Information("Application has started.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
