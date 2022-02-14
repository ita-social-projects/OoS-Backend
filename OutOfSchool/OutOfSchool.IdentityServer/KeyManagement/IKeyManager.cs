using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public interface IKeyManager
    {
        /// <summary>
        /// Returns current caches certificate or forces its re-initialization.
        /// </summary>
        /// <returns>An future containing <see cref="X509Certificate2"/>.</returns>
        public Task<X509Certificate2> Get();
    }
}