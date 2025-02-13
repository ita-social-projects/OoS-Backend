namespace OutOfSchool.AikomApiClient.Config;

public class AikomApiClientConfig
{
    public const string Name = "AikomApiClient";

    public bool Enable {  get; set; }

    public required string ApiUrl { get; set; }

    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }

    public required string TokenEndpoint { get; set; }
}
