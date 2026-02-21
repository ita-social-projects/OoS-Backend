using System.Security.Cryptography.X509Certificates;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

[Obsolete("Using externally generated certificates")]
public interface IKeyManager
{
    /// <summary>
    /// Returns current caches certificate or forces its re-initialization.
    /// </summary>
    /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
    public Task<X509Certificate2> Get(CertificateType type);
}