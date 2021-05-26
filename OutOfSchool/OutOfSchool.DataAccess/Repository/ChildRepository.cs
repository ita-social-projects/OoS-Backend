using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ChildRepository : EntityRepository<Child>, IChildRepository
    {
        private readonly OutOfSchoolDbContext db;

        public ChildRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        public new async Task<Child> Create(Child entity)
        {
            var address = await db.Addresses.AddAsync(entity.Address).ConfigureAwait(false);
            await db.SaveChangesAsync();

            entity.AddressId = address.Entity.Id;

            entity.Address = default;

            await db.Children.AddAsync(entity);
            await db.SaveChangesAsync();

            return await Task.FromResult(entity);
        }
    }
}
