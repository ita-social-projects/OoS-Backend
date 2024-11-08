namespace OutOfSchool.AuthCommon.Config;

public class AuthorizationServerConfig
{
    public const string Name = "AuthorizationServer";

    public string IntrospectionSecret { get; set; }

    public string[] AllowedCorsOrigins { get; set; }

    public AuthorizationCertificateConfig Certificate { get; set; }

    public OpenIdClient[] OpenIdClients { get; set; }

    public ExternalLogin ExternalLogin { get; set; }
}