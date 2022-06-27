using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.SubordinationStructure;

namespace OutOfSchool.Services.Repository
{
    public class InstitutionAdminRepository : EntityRepository<InstitutionAdmin>, IInstitutionAdminRepository
    {
        private readonly OutOfSchoolDbContext db;

        public InstitutionAdminRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<InstitutionAdmin> GetByIdAsync(string id, Guid providerId)
        {
            // FIXME
            return await db.InstitutionAdmins
            // .Where(ia => ia.Id == providerId)
            .SingleOrDefaultAsync(pa => pa.UserId == id);
        }

        public async Task<bool> IsExistInstitutionAdminDeputyWithUserIdAsync(Guid providerId, string userId)
        {
            var InstitutionAdmin = await GetByIdAsync(userId, providerId);

            return InstitutionAdmin != null; // && InstitutionAdmin.IsDeputy == true;
        }

        public async Task<bool> IsExistInstitutionWithUserIdAsync(string userId)
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
