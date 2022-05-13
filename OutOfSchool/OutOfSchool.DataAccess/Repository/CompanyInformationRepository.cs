using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class CompanyInformationRepository : SensitiveEntityRepository<AboutPortal>, ICompanyInformationRepository
    {
        private readonly OutOfSchoolDbContext db;

        public CompanyInformationRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<AboutPortal> GetWithNavigationsByTypeAsync(CompanyInformationType type)
        {
            return await db.AboutPortal
                .Where(ap => ap.Type == type)
                .Include(ap => ap.AboutPortalItems)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public void DeleteAllItemsByEntityAsync(AboutPortal entity)
        {
            db.AboutPortalItems.RemoveRange(db.AboutPortalItems.Where(api => api.AboutPortalId == entity.Id));
        }

        public async Task CreateItems(IEnumerable<AboutPortalItem> entities)
        {
            await db.AboutPortalItems.AddRangeAsync(entities).ConfigureAwait(false);
        }
    }
}
