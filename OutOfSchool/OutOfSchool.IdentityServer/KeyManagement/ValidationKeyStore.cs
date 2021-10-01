using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class ValidationKeyStore : IValidationKeysStore
    {
        private readonly IKeyManager keyManager;

        public ValidationKeyStore(IKeyManager manager)
        {
            keyManager = manager;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            try
            {
                var certificate = await keyManager.Get();

                var credential = certificate.ConvertToCredentials();

                var keyInfo = new SecurityKeyInfo
                {
                    Key = credential.Key,
                    SigningAlgorithm = credential.Algorithm,
                };

                return new[] {keyInfo};
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Array.Empty<SecurityKeyInfo>();
            }
        }
    }
}