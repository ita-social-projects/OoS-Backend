using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

public class OpenIdDictServerOptionsInitializer : IConfigureNamedOptions<OpenIddictServerOptions>
{
    private readonly IKeyManager keyManager;
    private readonly ILogger<OpenIdDictServerOptionsProvider> logger;

    public OpenIdDictServerOptionsInitializer(IKeyManager keyManager, ILogger<OpenIdDictServerOptionsProvider> logger)
    {
        this.keyManager = keyManager;
        this.logger = logger;
    }

    public void Configure(string name, OpenIddictServerOptions options) => Configure(options);

    public void Configure(OpenIddictServerOptions options)
    {
        X509Certificate2 signing;
        X509Certificate2 encryption;
        try
        {
            signing = keyManager.Get(CertificateType.Signing).Result;
            encryption = keyManager.Get(CertificateType.Encryption).Result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get keys");
            return;
        }

        // We know it's empty because we're not calling any helper methods in setup
        // otherwise need to probably add List.Clear() just in case
        options.SigningCredentials.Add(signing.ConvertToSigningCredentials());
        options.EncryptionCredentials.Add(encryption.ConvertToEncryptingCredentials());
    }
}