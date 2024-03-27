namespace OutOfSchool.WebApi.Config;

public class AuthorizationServerConfig
{
    public const string Name = "AuthorizationServer";

    public Uri Authority { get; set; }

    public string ApiName { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }
}