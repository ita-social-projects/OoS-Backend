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

        public async Task<MinistryAdmin> GetByIdAsync(string userId)
        {
            return await db.MinistryAdmins
                .SingleOrDefaultAsync(pa => pa.UserId == userId);
        }
    }
}
