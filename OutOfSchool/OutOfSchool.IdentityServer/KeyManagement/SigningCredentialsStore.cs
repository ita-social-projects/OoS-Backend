using System;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace OutOfSchool.IdentityServer.KeyManagement;

public class SigningCredentialsStore : ISigningCredentialStore
{
    private readonly IKeyManager keyManager;
    private readonly ILogger<SigningCredentialsStore> logger;

    public SigningCredentialsStore(IKeyManager manager, ILogger<SigningCredentialsStore> logger)
    {
        keyManager = manager;
        this.logger = logger;
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
            logger.LogError(e, "Failed to get signing credentials");
            return null;
        }
    }
}