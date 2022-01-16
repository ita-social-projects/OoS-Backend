using System;
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

        public async Task<ProviderAdmin> GetByIdAsync(string id, Guid providerId)
        {
            return await db.ProviderAdmins
                .Where(pa => pa.ProviderId == providerId)
                .SingleOrDefaultAsync(pa => pa.UserId == id);
        }

        public async Task<bool> IsExistProviderAdminDeputyWithUserIdAsync(Guid providerId, string userId)
        {
            var providerAdmin = await GetByIdAsync(userId, providerId);

            return providerAdmin != null && providerAdmin.IsDeputy == true;
        }

        public async Task<bool> IsExistProviderWithUserIdAsync(Guid providerId, string userId)
        {
            var provider = await db.Providers
                .SingleOrDefaultAsync(p => p.UserId == userId);

            return provider != null;
        }

        public async Task<int> GetNumberProviderAdminsAsync(Guid providerId)
        {
            return await db.ProviderAdmins
                .CountAsync(pa => pa.ProviderId == providerId);
        }
    }
}
