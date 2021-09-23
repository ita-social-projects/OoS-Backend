using System;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class SigningCertificate
    {
        public SigningCertificate()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string CertificateBase64 { get; set; }

        public DateTimeOffset ExpirationDate { get; set; }
    }
}