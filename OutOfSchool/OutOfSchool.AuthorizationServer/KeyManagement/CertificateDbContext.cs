namespace OutOfSchool.AuthorizationServer.KeyManagement;

public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options)
        : base(options)
    {
    }

    public DbSet<SigningOrEncryptionCertificate> Certificates { get; set; }
}