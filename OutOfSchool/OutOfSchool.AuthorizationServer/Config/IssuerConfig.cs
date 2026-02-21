namespace OutOfSchool.AuthorizationServer.Config;

[Obsolete("Using externally generated certificates")]
public class IssuerConfig
{
    public static readonly string Name = "Issuer";

    public string Uri { get; set; }

    public int CertificateExpirationDays { get; set; }
}