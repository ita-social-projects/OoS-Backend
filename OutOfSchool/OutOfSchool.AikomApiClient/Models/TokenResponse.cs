using System.Text.Json.Serialization;

namespace OutOfSchool.AikomApiClient.Models;

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
