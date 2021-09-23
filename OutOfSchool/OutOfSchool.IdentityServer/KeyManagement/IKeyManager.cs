using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public interface IKeyManager
    {
        /// <summary>
        /// Returns current caches certificate or forces its re-initialization.
        /// </summary>
        /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
        public Task<X509Certificate2> Get();

        /// <summary>
        /// Converts <see cref="X509Certificate2"/> into a valid <see cref="SigningCredentials"/> for
        /// Identity Server to use for signing and validating tokens.
        /// </summary>
        /// <param name="cert">Application X509 signing certificate.</param>
        /// <param name="signingAlgorithm">Signing algorithm, defaults to SHA256.</param>
        /// <returns>Signing credentials for Identity Server.</returns>
        public SigningCredentials ConvertToCredentials(
            X509Certificate2 cert,
            string signingAlgorithm = SecurityAlgorithms.RsaSha256);
    }
}