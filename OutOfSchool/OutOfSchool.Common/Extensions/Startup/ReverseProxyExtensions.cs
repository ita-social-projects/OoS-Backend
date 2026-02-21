using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.Common.Config;

namespace OutOfSchool.Common.Extensions.Startup;

public static class ReverseProxyExtensions
{
    public static IServiceCollection AddProxy(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    public static IApplicationBuilder UseProxy(this IApplicationBuilder app, ReverseProxyOptions options)
    {
        var basePath = options.BasePath;

        app.UseForwardedHeaders();

        if (!string.IsNullOrEmpty(basePath))
        {
            app.Use(async (context, next) =>
            {
                context.Request.PathBase = basePath;
                await next.Invoke()
                    .ConfigureAwait(false);
            });
        }

        return app;
    }
}