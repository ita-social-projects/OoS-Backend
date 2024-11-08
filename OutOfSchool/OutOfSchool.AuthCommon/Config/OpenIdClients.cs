namespace OutOfSchool.AuthCommon.Config;

public class OpenIdClient
{
    public string ClientId { get; set; }

    public string[] RedirectUris { get; set; }

    public string[] PostLogoutRedirectUris { get; set; }

    public string DisplayName { get; set; }

    public Dictionary<string, string> DisplayNames { get; set; }

    public bool IsIntrospection { get; set; }
}