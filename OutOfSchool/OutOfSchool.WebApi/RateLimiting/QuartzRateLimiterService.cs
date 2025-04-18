using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;
using RateLimiterOptions = OutOfSchool.WebApi.Config.RateLimiterOptions;

namespace OutOfSchool.WebApi.RateLimiting;

/// <summary>
/// Service implementation for Quartz monitoring rate limiting.
/// </summary>
public class QuartzRateLimiterService : IRateLimiterService
{
    private readonly RateLimiterOptions options;
    private readonly ILogger<QuartzRateLimiterService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="QuartzRateLimiterService"/> class.
    /// </summary>
    /// <param name="options">The rate limiter options.</param>
    /// <param name="logger">The logger.</param>
    public QuartzRateLimiterService(
        IOptions<RateLimiterOptions> options,
        ILogger<QuartzRateLimiterService> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public RateLimitPartition<string> GetPolicy(HttpContext context)
    {
        string partitionKey = GetPartitionKey(context, options);

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = options.PermitLimit,
                Window = TimeSpan.FromSeconds(options.Window),
                QueueLimit = options.QueueLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    }

    /// <inheritdoc/>
    public Task HandleRejection(OnRejectedContext context, CancellationToken token)
    {
        logger.LogWarning(
            "Rate limit exceeded for client {ClientIP}, user {Username}, endpoint {Path}",
            context.HttpContext.Connection.RemoteIpAddress,
            context.HttpContext.User.Identity?.Name ?? "anonymous",
            context.HttpContext.Request.Path);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.Append(
            "Retry-After", 
            DateTime.UtcNow.AddSeconds(options.RetryAfterSeconds)
            .ToString("R"));

        return context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Error = $"Rate limit exceeded. You can make another request in {options.RetryAfterSeconds} seconds.",
            RetryAfter = options.RetryAfterSeconds
        }, token);
    }

    /// <summary>
    /// Gets the appropriate partition key based on configuration and request context.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="options">The rate limiter options.</param>
    /// <returns>The partition key.</returns>
    private static string GetPartitionKey(HttpContext httpContext, RateLimiterOptions options)
    {
        if (options.EnableIpBasedRateLimiting)
        {
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip";
        }

        return httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.Identity.Name ?? "anonymous"
            : "anonymous";
    }
}
