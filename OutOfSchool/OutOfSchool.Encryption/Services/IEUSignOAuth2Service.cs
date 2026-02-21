using OutOfSchool.Common.Models.ExternalAuth;

namespace OutOfSchool.Encryption.Services;

/// <summary>
/// This interface defines two primary methods for handling encryption certificates and decrypting user information
/// in the context of OAuth 2.0 authentication, specifically using the IIT End User Library for Ukrainian government services.
/// </summary>
public interface IEUSignOAuth2Service
{
    /// <summary>
    /// Get Base64 encoded encryption certificate.
    /// When using this certificate as a query string parameter, it should be DOUBLE Url encoded due to the nature of
    /// Base64 encoding (usage of symbols that have special meaning in URI, like `=`).
    /// E.g., Uri.EscapeDataString(Uri.EscapeDataString(cert)).
    ///
    /// Note: Our CommunicationService also has QueryHelpers.AddQueryString call. Keep that in mind.
    /// </summary>
    /// <returns>A <see cref="CertificateResponse"/> wrapper containing the certificate.</returns>
    public CertificateResponse GetEnvelopeCertificateBase64();

    /// <summary>
    /// Decrypts user information using IIT End User Library.
    /// </summary>
    /// <param name="encryptedUserInfo">Encrypted user information from id.gov.ua/get-user-info API.</param>
    /// <returns>A <see cref="UserInfoResponse"/> representing available user information.</returns>
    public UserInfoResponse DecryptUserInfo(EnvelopedUserInfoResponse encryptedUserInfo);
}