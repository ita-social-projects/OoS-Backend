namespace OutOfSchool.AuthorizationServer;

public class OpenIdDictDbContext : DbContext, IUnitOfWork
{
    public OpenIdDictDbContext(DbContextOptions<OpenIdDictDbContext> options)
        : base(options)
    {
    }

    public async Task<int> CompleteAsync() => await this.SaveChangesAsync();

    public int Complete() => this.SaveChanges();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder
            .UseOpenIddict();
    }
}