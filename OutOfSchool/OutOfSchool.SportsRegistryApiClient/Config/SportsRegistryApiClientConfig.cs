namespace OutOfSchool.SportsRegistryApiClient.Config;

public class SportsRegistryApiClientConfig
{
    public const string Name = "SportsRegistryApiClient";

    public bool Enable {  get; set; }
    /// <summary>
    /// Base URL of the Sports Registry API (e.g., https://external-service-api...)
    /// </summary>
    public required string ApiUrl { get; init; }

    /// <summary>
    /// URL for obtaining an access token (Keycloak token endpoint)
    /// </summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>
    /// OAuth2 client ID for authorization
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// OAuth2 client secret for authorization
    /// </summary>
    public required string ClientSecret { get; init; }
}