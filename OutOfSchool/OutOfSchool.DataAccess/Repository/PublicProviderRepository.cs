using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;
public class PublicProviderRepository : ProviderFakeRepository<PublicProvider>, IPublicProviderRepository
{
    private readonly OutOfSchoolDbContext db;
    private readonly DbSet<PublicProvider> providers;

    public PublicProviderRepository(OutOfSchoolDbContext dbContext) : base(dbContext)
    {
        db = dbContext;
        providers = dbContext.PublicProviders;
    }
}
