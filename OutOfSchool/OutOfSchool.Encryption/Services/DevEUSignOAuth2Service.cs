using System.Text.Json;
using OutOfSchool.Encryption.Models;

namespace OutOfSchool.Encryption.Services;

public class DevEUSignOAuth2Service : IEUSignOAuth2Service
{
    /// <inheritdoc/>
    public CertificateResponse GetEnvelopCertificateBase64() => new()
    {
        CertBase64 = Convert.ToBase64String("very_secret_certificate"u8.ToArray()),
    };

    /// <inheritdoc/>
    public UserInfoResponse DecryptUserInfo(EnvelopedUserInfoResponse encryptedUserInfo) =>

        // Mock local auth server sends data as unencrypted string.
        JsonSerializer.Deserialize<UserInfoResponse>(encryptedUserInfo.EncryptedUserInfo);
}