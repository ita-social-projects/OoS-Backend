using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ProviderAdminRepository : EntityRepository<ProviderAdmin>, IProviderAdminRepository
    {
        private readonly OutOfSchoolDbContext db;

        public ProviderAdminRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<bool> IsExistProviderAdminWithUserIdAsync(long providerId, string userId)
        {
            var providerAdmin = await db.ProviderAdmins
                .Where(pa => pa.ProviderId == providerId)
                .SingleOrDefaultAsync(pa => pa.UserId == userId);

            return providerAdmin != null;
        }

        public async Task<bool> IsExistProviderWithUserIdAsync(long providerId, string userId)
        {
            var provider = await db.Providers
                .Where(p => p.Id == providerId)
                .SingleOrDefaultAsync(p => p.UserId == userId);

            return provider != null;
        }

        public async Task<int> GetNumberProviderAdminsAsync(long providerId)
        {
            var number = await db.ProviderAdmins
                .Where(pa => pa.ProviderId == providerId)
                .CountAsync();

            return number;
        }
    }
}
