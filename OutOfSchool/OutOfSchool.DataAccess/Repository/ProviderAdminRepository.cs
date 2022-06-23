using System;
using System.Collections.Generic;
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

        public async Task<ProviderAdmin> GetByIdAsync(string userId, Guid providerId)
        {
            return await db.ProviderAdmins
                .Where(pa => pa.ProviderId == providerId)
                .SingleOrDefaultAsync(pa => pa.UserId == userId);
        }

        public async Task<bool> IsExistProviderAdminDeputyWithUserIdAsync(Guid providerId, string userId)
        {
            var providerAdmin = await GetByIdAsync(userId, providerId);

            return providerAdmin != null && providerAdmin.IsDeputy;
        }

        public async Task<bool> IsExistProviderWithUserIdAsync(string userId)
        {
            var provider = await db.Providers
                .SingleOrDefaultAsync(p => p.UserId == userId);

            return provider != null;
        }

        public async Task<Provider> GetProviderWithUserIdAsync(string userId)
        {
            var provider = await db.Providers
                .SingleOrDefaultAsync(p => p.UserId == userId);

            return provider;
        }

        public async Task AddRelatedWorkshopForAssistant(string userId, Guid workshopId)
        {
            var providerAdmin = await db.ProviderAdmins.SingleOrDefaultAsync(p => p.UserId == userId);
            var workshopToUpdate = await db.Workshops.SingleOrDefaultAsync(w => w.Id == workshopId);
            workshopToUpdate.ProviderAdmins = new List<ProviderAdmin> { providerAdmin };
            db.Update(workshopToUpdate);
            await db.SaveChangesAsync();
        }

        public async Task<int> GetNumberProviderAdminsAsync(Guid providerId)
        {
            return await db.ProviderAdmins
                .CountAsync(pa => pa.ProviderId == providerId);
        }
    }
}
