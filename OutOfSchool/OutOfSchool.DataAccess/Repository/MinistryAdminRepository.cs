using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository
{
    public class MinistryAdminRepository : EntityRepository<MinistryAdmin>, IMinistryAdminRepository
    {
        private readonly OutOfSchoolDbContext db;

        public MinistryAdminRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<MinistryAdmin> GetByIdAsync(string id, Guid providerId)
        {
            // FIXME
            return await db.MinistryAdmins
            // .Where(ia => ia.Id == providerId)
            .SingleOrDefaultAsync(pa => pa.UserId == id);
        }

        public async Task<bool> IsExistMinistryAdminDeputyWithUserIdAsync(Guid providerId, string userId)
        {
            var InstitutionAdmin = await GetByIdAsync(userId, providerId);

            return InstitutionAdmin != null; // && InstitutionAdmin.IsDeputy == true;
        }

        public async Task<bool> IsExistMinistryWithUserIdAsync(string userId)
        {
            var provider = await db.Providers
                .SingleOrDefaultAsync(p => p.UserId == userId);

            return provider != null;
        }

        public async Task<Institution> GetInstitutionWithUserIdAsync(string userId)
        {
            // FIXME
            var institution = await db.Institutions
                .SingleOrDefaultAsync(); // p => p.UserId == userId);

            return institution;
        }

        public async Task AddRelatedWorkshopForAssistant(string userId, Guid workshopId)
        {
            // FIXME 
            /* 
            var InstitutionAdmin = await db.InstitutionAdmins.SingleOrDefaultAsync(p => p.UserId == userId);
            var workshopToUpdate = await db.Workshops.SingleOrDefaultAsync(w => w.Id == workshopId);
            workshopToUpdate.InstitutionAdmins = new List<InstitutionAdmin> { InstitutionAdmin };
            db.Update(workshopToUpdate);
            await db.SaveChangesAsync();
            */ 
        }
    }
}
