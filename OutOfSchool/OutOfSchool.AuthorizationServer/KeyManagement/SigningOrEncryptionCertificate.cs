using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

public class SigningOrEncryptionCertificate
{
    public string Id { get; set; }

    public string CertificateBase64 { get; set; }

    public DateTimeOffset ExpirationDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}