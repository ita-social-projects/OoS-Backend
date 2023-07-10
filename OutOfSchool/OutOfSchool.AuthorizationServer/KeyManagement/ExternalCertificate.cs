using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OutOfSchool.AuthorizationServer.KeyManagement;

public class ExternalCertificate
{
    public static async Task<X509Certificate2> LoadCertificates(IConfiguration configuration)
    {
        var certificateConfiguration = configuration.GetSection("AuthorizationServer:Certificate");
        var folderPath = certificateConfiguration["Folder"];
        var certificateFileName = certificateConfiguration["PemFileName"];
        var privateKeyFileName = certificateConfiguration["PrivateKeyFileName"];

        // OpenSSL / Cert-Manager / Kubernetes-style Certificates
        if (!string.IsNullOrEmpty(certificateFileName) && !string.IsNullOrEmpty(privateKeyFileName))
        {
            var certificatePath = Path.Combine(folderPath, certificateFileName);
            var privateKeyPath = Path.Combine(folderPath, privateKeyFileName);
            return await LoadPemCertificate(certificatePath, privateKeyPath);
        }

        // Windows-style Certificate
        // This should never happen, but just in case we need it sometime
        var pfxPath = Path.Combine(folderPath, certificateConfiguration["PfxFileName"]);
        var pfxPassword = certificateConfiguration["PfxPassword"];
        return new X509Certificate2(pfxPath, pfxPassword);
    }

    private static async Task<X509Certificate2> LoadPemCertificate(string certificatePath, string privateKeyPath)
    {
        using var publicKey = new X509Certificate2(certificatePath);

        var privateKeyText = await File.ReadAllTextAsync(privateKeyPath);
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