namespace OutOfSchool.WebApi.Config;

public class IdentityServerConfig
{
    public const string Name = "Identity";

    public Uri Authority { get; set; }

    public string ApiName { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public bool EnableOpenIdDict { get; set; }
}