using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.Encryption.Config;

public class PrivateKey
{
    [Required]
    public string FileName { get; set; }

    public string JKSAlias { get; set; }

    public string MediaType { get; set; }

    public string MediaDevice { get; set; }

    public string Password { get; set; }

    public string[] CertificateFilePaths { get; set; }

    public string CAIssuerCN { get; set; }
}