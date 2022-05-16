using Google.Apis.Auth.OAuth2;
using OutOfSchool.Services.Contexts.Configuration;

namespace OutOfSchool.Services.Extensions
{
    public static class GcpExtension
    {
        public static GoogleCredential RetrieveGoogleCredential(this GcpStorageSourceConfig config)
        {
            if (string.IsNullOrEmpty(config.CredentialFile))
            {
                return GoogleCredential.GetApplicationDefault();
            }

            return GoogleCredential.FromFile(config.CredentialFile);
        }
    }
}