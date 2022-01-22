using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class ValidationKeyStore : IValidationKeysStore
    {
        private readonly IKeyManager keyManager;
        private readonly ILogger<ValidationKeyStore> logger;

        public ValidationKeyStore(IKeyManager manager, ILogger<ValidationKeyStore> logger)
        {
            keyManager = manager;
            this.logger = logger;
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
                logger.LogError(e, "Failed to get validation keys");
                return Array.Empty<SecurityKeyInfo>();
            }
        }
    }
}