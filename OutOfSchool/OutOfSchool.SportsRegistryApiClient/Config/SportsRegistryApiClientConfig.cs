namespace OutOfSchool.SportsRegistryApiClient.Config;

public class SportsRegistryApiClientConfig
{
    public const string SectionName = "SportsRegistryApiClient";

    /// <summary>
    /// Base URL of the Sports Registry API (e.g., https://external-service-api...)
    /// </summary>
    public required string ApiBaseUrl { get; init; }

    /// <summary>
    /// URL for obtaining an access token (Keycloak token endpoint)
    /// </summary>
    public required string TokenUrl { get; init; }

    /// <summary>
    /// OAuth2 client ID for authorization
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// OAuth2 client secret for authorization
    /// </summary>
    public required string ClientSecret { get; init; }
}