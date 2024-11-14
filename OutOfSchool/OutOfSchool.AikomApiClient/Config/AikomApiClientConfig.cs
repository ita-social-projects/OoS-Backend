namespace OutOfSchool.AikomApiClient.Config;

public class AikomApiClientConfig
{
    public const string Name = "AikomApiClient";

    public bool Enable {  get; set; }

    public string ApiUrl { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string TokenEndpoint { get; set; }

    public string AuthorizationEndpoint {  get; set; }
}
