using System;
using System.ComponentModel.DataAnnotations;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class SigningCertificate
    {
        public string Id { get; set; }

        public string CertificateBase64 { get; set; }

        public DateTimeOffset ExpirationDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}