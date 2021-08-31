using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.Common.Config;

namespace OutOfSchool.Common.Extensions.Startup
{
    /// <summary>
    ///     Reverse Proxy Extensions.
    /// </summary>
    public static class ReverseProxyExtensions
    {
        /// <summary>
        ///     Add Proxy.
        /// </summary>
        public static IServiceCollection AddProxy(this IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            return services;
        }

        /// <summary>
        ///     Use Proxy.
        /// </summary>
        public static IApplicationBuilder UseProxy(this IApplicationBuilder app, ReverseProxyOptions options)
        {
            var basePath = options.BasePath;

            if (!string.IsNullOrEmpty(basePath))
            {
                app.Use(async (context, next) =>
                {
                    context.Request.PathBase = basePath;
                    await next.Invoke()
                        .ConfigureAwait(false);
                });
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
            });

            return app;
        }
    }
}