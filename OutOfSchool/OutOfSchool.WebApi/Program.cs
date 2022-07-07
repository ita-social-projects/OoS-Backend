var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration));

GlobalLogContext.PushProperty("AppVersion", builder.Configuration.GetSection("AppDefaults:Version").Value);

// Add services to the container.
builder.AddApplicationServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.Configure();

try
{
    Log.Information("Application has started.");
    app.Run();
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
