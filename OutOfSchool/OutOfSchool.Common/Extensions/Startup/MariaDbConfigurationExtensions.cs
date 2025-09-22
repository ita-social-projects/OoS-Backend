using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace OutOfSchool.Common.Extensions.Startup;

public static class MariaDbConfigurationExtensions
{
    /// <summary>
    /// Retrieves and validates the MariaDB server version from IConfiguration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A MariaDbServerVersion object to be used in UseMySql.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the version is not configured or is lower than the minimum allowed.
    /// </exception>
    public static MariaDbServerVersion GetAndValidateMariaDbVersion(this IConfiguration configuration)
    {
        var mariaDbServerVersion = configuration[Constants.MariaDbServerVersion];
        if (string.IsNullOrWhiteSpace(mariaDbServerVersion))
        {
            throw new InvalidOperationException("MariaDbServerVersion is not configured.");
        }

        var serverVersion = new MariaDbServerVersion(new Version(mariaDbServerVersion));

        if (serverVersion.Version.Major < Constants.MariaDbServerMinimalMajorVersion)
        {
            throw new InvalidOperationException(
                $"MariaDb Server version should be {Constants.MariaDbServerMinimalMajorVersion} or higher.");
        }

        return serverVersion;
    }
}