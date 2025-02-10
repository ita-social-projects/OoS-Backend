namespace OutOfSchool.AuthCommon.Config;

public class AuthorizationCertificateConfig
{
    public const string Name = "Certificate";

    public string Folder { get; set; }

    public string? PemFileName { get; set; }

    public string? PrivateKeyFileName { get; set; }

    public string? PfxFileName { get; set; }

    public string? PfxPassword { get; set; }
}