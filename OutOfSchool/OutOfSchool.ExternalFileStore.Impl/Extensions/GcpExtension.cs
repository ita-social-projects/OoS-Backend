using Google.Apis.Auth.OAuth2;
using OutOfSchool.ExternalFileStore.Config;

namespace OutOfSchool.ExternalFileStore.Extensions;

public static class GcpExtension
{
    public static GoogleCredential RetrieveGoogleCredential(this GoogleCloudConfig config)
    {
        return string.IsNullOrEmpty(config.CredentialFilePath)
            ? GoogleCredential.GetApplicationDefault()
            : GoogleCredential.FromFile(config.CredentialFilePath);
    }
}