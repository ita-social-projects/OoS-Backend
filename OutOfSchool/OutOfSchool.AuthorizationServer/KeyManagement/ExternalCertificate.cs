using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using OutOfSchool.AuthCommon.Config;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

public static class ExternalCertificate
{
    public static X509Certificate2 LoadCertificates(AuthorizationCertificateConfig configuration)
    {
        var folderPath = configuration.Folder;
        var certificateFileName = configuration.PemFileName;
        var privateKeyFileName = configuration.PrivateKeyFileName;

        // OpenSSL / Cert-Manager / Kubernetes-style Certificates
        if (!string.IsNullOrEmpty(certificateFileName) && !string.IsNullOrEmpty(privateKeyFileName))
        {
            var certificatePath = Path.Combine(folderPath, certificateFileName);
            var privateKeyPath = Path.Combine(folderPath, privateKeyFileName);
            return LoadPemCertificate(certificatePath, privateKeyPath);
        }

        // Windows-style Certificate
        // This should never happen, but just in case we need it sometime
        var pfxFileName = configuration.PfxFileName;
        if (!string.IsNullOrEmpty(pfxFileName))
        {
            var pfxPath = Path.Combine(folderPath, pfxFileName);
            var pfxPassword = configuration.PfxPassword;
            return new X509Certificate2(pfxPath, pfxPassword);
        }

        // This also should not happen :)
        throw new ArgumentException("Configuration does not contain valid entries", nameof(configuration));
    }

    private static X509Certificate2 LoadPemCertificate(string certificatePath, string privateKeyPath)
    {
        using var publicKey = new X509Certificate2(certificatePath);

        var privateKeyText = File.ReadAllText(privateKeyPath);
        var privateKeyBlocks = privateKeyText.Split("-", StringSplitOptions.RemoveEmptyEntries);
        var privateKeyBytes = Convert.FromBase64String(privateKeyBlocks[1]);
        using var rsa = RSA.Create();

        switch (privateKeyBlocks[0])
        {
            case "BEGIN PRIVATE KEY":
                rsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
                break;
            case "BEGIN RSA PRIVATE KEY":
                rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                break;
        }

        var keyPair = publicKey.CopyWithPrivateKey(rsa);
        return new X509Certificate2(keyPair.Export(X509ContentType.Pfx));
    }
}