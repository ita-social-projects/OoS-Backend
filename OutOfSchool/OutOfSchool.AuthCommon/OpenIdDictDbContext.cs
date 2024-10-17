namespace OutOfSchool.AuthCommon;

public class OpenIdDictDbContext : DbContext
{
    public OpenIdDictDbContext(DbContextOptions<OpenIdDictDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .UseOpenIddict();
    }
}