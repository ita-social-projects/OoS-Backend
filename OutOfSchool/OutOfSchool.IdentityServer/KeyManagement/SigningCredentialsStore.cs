using System;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class SigningCredentialsStore : ISigningCredentialStore
    {
        private readonly KeyManager keyManager;

        public SigningCredentialsStore(KeyManager manager)
        {
            keyManager = manager;
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            try
            {
                var certificate = await keyManager.Get();

                return keyManager.ConvertToCredentials(certificate);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}