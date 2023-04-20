using Microsoft.EntityFrameworkCore;

namespace OutOfSchool.IdentityServer.KeyManagement;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options)
        : base(options)
    {
    }

    public DbSet<SigningCertificate> Certificates { get; set; }
}