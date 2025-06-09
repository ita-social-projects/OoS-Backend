namespace OutOfSchool.WebApi.Config;

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimiterOptions
{
    /// <summary>
    /// Maximum number of requests allowed in the specified window.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Time window in seconds for the rate limiting.
    /// </summary>
    public int Window { get; set; }

    /// <summary>
    /// Maximum size of the queue for pending requests (0 means no queue).
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Whether to use client IP address as the partition key.
    /// If false, authenticated username will be used if available.
    /// </summary>
    public bool EnableIpBasedRateLimiting { get; set; }

    /// <summary>
    /// Time in seconds to wait before retrying after a rate limit is exceeded.
    /// </summary>
    public int RetryAfterSeconds { get; set; }
}
