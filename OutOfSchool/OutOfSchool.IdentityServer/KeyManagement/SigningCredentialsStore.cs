using System;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.IdentityModel.Tokens;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class SigningCredentialsStore : ISigningCredentialStore
    {
        private readonly IKeyManager keyManager;

        public SigningCredentialsStore(IKeyManager manager)
        {
            keyManager = manager;
        }

        /// <inheritdoc />
        public async Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            try
            {
                var certificate = await keyManager.Get();

                return certificate.ConvertToCredentials();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}