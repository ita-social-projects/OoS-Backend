using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OutOfSchool.Encryption.Models;

namespace OutOfSchool.Encryption.Services;

/// <inheritdoc/>
public class DevEUSignOAuth2Service : IEUSignOAuth2Service
{
    /// <inheritdoc/>
    public CertificateResponse GetEnvelopeCertificateBase64() => new()
    {
        CertBase64 = Convert.ToBase64String("very_secret_certificate"u8.ToArray()),
    };

    /// <inheritdoc/>
    public UserInfoResponse DecryptUserInfo([NotNull] EnvelopedUserInfoResponse encryptedUserInfo)
    {
        if (encryptedUserInfo == null)
        {
            return null;
        }

        // Mock local auth server sends data as unencrypted string.
        return JsonSerializer.Deserialize<UserInfoResponse>(encryptedUserInfo.EncryptedUserInfo);
    }
}