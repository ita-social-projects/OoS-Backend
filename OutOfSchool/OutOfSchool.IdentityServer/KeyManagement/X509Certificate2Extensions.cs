using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace OutOfSchool.IdentityServer.KeyManagement;

public static class X509Certificate2Extensions
{
    /// <summary>
    /// Converts <see cref="X509Certificate2"/> into a valid <see cref="SigningCredentials"/> for
    /// Identity Server to use for signing and validating tokens.
    /// </summary>
    /// <param name="cert">Application X509 signing certificate.</param>
    /// <param name="signingAlgorithm">Signing algorithm, defaults to SHA256.</param>
    /// <returns>Signing credentials for Identity Server.</returns>
    public static SigningCredentials ConvertToCredentials(
        this X509Certificate2 cert,
        string signingAlgorithm = SecurityAlgorithms.RsaSha256)
    {
        var key = new X509SecurityKey(cert);
        key.KeyId += signingAlgorithm;

        return new SigningCredentials(key, signingAlgorithm);
    }
}