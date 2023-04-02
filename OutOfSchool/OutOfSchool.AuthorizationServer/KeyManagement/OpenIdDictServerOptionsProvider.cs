using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

public class OpenIdDictServerOptionsProvider : IOptionsMonitor<OpenIddictServerOptions>
{
    private readonly IKeyManager keyManager;
    private readonly IOptionsFactory<OpenIddictServerOptions> optionsFactory;
    private readonly ILogger<OpenIdDictServerOptionsProvider> logger;

    public OpenIdDictServerOptionsProvider(
        IKeyManager keyManager,
        IOptionsFactory<OpenIddictServerOptions> optionsFactory,
        ILogger<OpenIdDictServerOptionsProvider> logger)
    {
        this.keyManager = keyManager;
        this.optionsFactory = optionsFactory;
        this.logger = logger;
    }

    public OpenIddictServerOptions CurrentValue => Get(Options.DefaultName);

    public OpenIddictServerOptions Get(string name)
    {
        var options = optionsFactory.Create(name);
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
            return options;
        }

        var signingCredentials = signing.ConvertToSigningCredentials();
        if (options.SigningCredentials.Count > 1)
        {
            options.SigningCredentials.Clear();
        }

        options.SigningCredentials[0] = signingCredentials;

        var encryptionCredentials = encryption.ConvertToEncryptingCredentials();
        if (options.EncryptionCredentials.Count > 1)
        {
            options.EncryptionCredentials.Clear();
        }

        options.EncryptionCredentials[0] = encryptionCredentials;

        return options;
    }

    public IDisposable OnChange(Action<OpenIddictServerOptions, string> listener) => null;
}