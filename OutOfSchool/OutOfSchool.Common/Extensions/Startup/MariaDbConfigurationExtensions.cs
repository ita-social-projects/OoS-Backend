using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace OutOfSchool.Common.Extensions.Startup;

public static class MariaDbConfigurationExtensions
{
    /// <summary>
    /// Retrieves and validates the MariaDB server version from IConfiguration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A System.Version representing the MariaDB server version.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the version is not configured, cannot be parsed, or is lower than the minimum allowed.
    /// </exception>
    public static Version GetAndValidateMariaDbVersion(this IConfiguration configuration)
    {
        var raw = configuration[Constants.MariaDbServerVersion];
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new InvalidOperationException("MariaDbServerVersion is not configured.");
        }

        // Extract the leading numeric part (e.g., "10.11.7" from "10.11.7-MariaDB-1:...")
        var numeric = new string(raw.Trim().TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
        if (!Version.TryParse(numeric, out var parsed))
        {
            throw new InvalidOperationException(
                $"Invalid MariaDbServerVersion value '{raw}'. Expected formats like '10.11' or '11.4.2'.");
        }

        // Allow MariaDB 10.11+ and 11+
        var isSupported =
            parsed.Major > 10 ||
            (parsed.Major == 10 && parsed.Minor >= 11);

        if (!isSupported)
        {
            throw new InvalidOperationException("MariaDb Server version should be 10.11 or higher.");
        }

        return parsed;
    }
}