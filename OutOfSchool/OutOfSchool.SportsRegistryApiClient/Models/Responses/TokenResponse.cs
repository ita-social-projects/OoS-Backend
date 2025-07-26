namespace OutOfSchool.SportsRegistryApiClient.Models.Responses;
using OutOfSchool.Common.Models;
public class TokenResponse: IResponse
{
    public string AccessToken { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = null!;
    public string Scope { get; set; } = null!;
}