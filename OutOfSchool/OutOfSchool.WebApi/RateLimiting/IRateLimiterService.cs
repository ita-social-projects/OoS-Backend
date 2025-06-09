using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace OutOfSchool.WebApi.RateLimiting;

/// <summary>
/// Service for handling rate limiting operations.
/// </summary>
public interface IRateLimiterService
{
    /// <summary>
    /// Gets the rate limiter policy for the given HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A rate limiter partition.</returns>
    RateLimitPartition<string> GetPolicy(HttpContext context);

    /// <summary>
    /// Handles rejected requests due to rate limiting.
    /// </summary>
    /// <param name="context">The rejection context.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task HandleRejection(OnRejectedContext context, CancellationToken token);
}
