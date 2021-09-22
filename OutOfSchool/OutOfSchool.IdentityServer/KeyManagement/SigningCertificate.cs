using System;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class SigningCertificate
    {
        public string CertificateBase64 { get; set; }

        public DateTimeOffset ExpirationDate { get; set; }
    }
}