namespace OutOfSchool.AuthorizationServer.KeyManagement;

[Obsolete("Using externally generated certificates")]
public class CertificateDbContext : DbContext
{
    public CertificateDbContext(DbContextOptions<CertificateDbContext> options)
        : base(options)
    {
    }

    public DbSet<SigningOrEncryptionCertificate> Certificates { get; set; }
}