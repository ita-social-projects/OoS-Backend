namespace OutOfSchool.AuthCommon;

public class OpenIdDictDbContext : DbContext, IUnitOfWork
{
    public OpenIdDictDbContext(DbContextOptions<OpenIdDictDbContext> options)
        : base(options)
    {
    }

    public Task<int> CompleteAsync() => this.SaveChangesAsync();

    public int Complete() => this.SaveChanges();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .UseOpenIddict();
    }
}