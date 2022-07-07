namespace OutOfSchool.IdentityServer.Config;

public class IssuerConfig
{
    public static readonly string Name = "Issuer";

    public string Uri { get; set; }

    public int CertificateExpirationDays { get; set; }
}