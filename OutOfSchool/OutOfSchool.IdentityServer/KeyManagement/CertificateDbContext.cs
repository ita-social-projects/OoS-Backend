using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.IdentityServer.KeyManagement
{
    public class CertificateDbContext : DbContext
    {
        public DbSet<SigningCertificate> Certificates { get; set; }
    }
}