namespace OutOfSchool.IdentityServer.Config;

public class AdditionalIdentityClients
{
    public string ClientId { get; set; }

    public string[] RedirectUris { get; set; }

    public string[] PostLogoutRedirectUris { get; set; }

    public string[] AllowedCorsOrigins { get; set; }
}