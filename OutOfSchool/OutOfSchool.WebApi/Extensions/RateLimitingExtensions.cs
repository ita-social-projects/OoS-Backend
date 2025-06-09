using OutOfSchool.WebApi.RateLimiting;

namespace OutOfSchool.WebApi.Extensions;

public static class RateLimiterExtensions
{
    /// <summary>
    /// Adds rate limiting services for Quartz monitoring.
    /// </summary>
    public static IServiceCollection AddQuartzMonitoringRateLimiter(this IServiceCollection services)
    {
        // Register the rate limiter service
        services.AddSingleton<IRateLimiterService, QuartzRateLimiterService>();

        services.AddRateLimiter(options =>
        {
            // Configure the rate limiter policy
            options.AddPolicy("QuartzMonitoringLimiter", httpContext =>
            {
                var service = httpContext.RequestServices.GetRequiredService<IRateLimiterService>();
                return service.GetPolicy(httpContext);
            });

            // Configure rejection handler
            options.OnRejected = async (context, token) =>
            {
                var service = context.HttpContext.RequestServices.GetRequiredService<IRateLimiterService>();
                await service.HandleRejection(context, token);
            };
        });

        return services;
    }
}