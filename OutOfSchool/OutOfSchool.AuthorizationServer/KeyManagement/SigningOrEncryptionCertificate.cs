using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

[Obsolete("Using externally generated certificates")]
public class SigningOrEncryptionCertificate
{
    public string Id { get; set; }

    public string CertificateBase64 { get; set; }

    public DateTimeOffset ExpirationDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}