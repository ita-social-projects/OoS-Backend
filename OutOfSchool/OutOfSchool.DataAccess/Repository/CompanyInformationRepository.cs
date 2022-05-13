using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class CompanyInformationRepository : SensitiveEntityRepository<CompanyInformation>, ICompanyInformationRepository
    {
        private readonly OutOfSchoolDbContext db;

        public CompanyInformationRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        public async Task<CompanyInformation> GetWithNavigationsByTypeAsync(CompanyInformationType type)
        {
            return await db.CompanyInformation
                .Where(ap => ap.Type == type)
                .Include(ap => ap.CompanyInformationItems)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public void DeleteAllItemsByEntityAsync(CompanyInformation entity)
        {
            db.CompanyInformationItems.RemoveRange(db.CompanyInformationItems.Where(api => api.AboutPortalId == entity.Id));
        }

        public async Task CreateItems(IEnumerable<CompanyInformationItem> entities)
        {
            await db.CompanyInformationItems.AddRangeAsync(entities).ConfigureAwait(false);
        }
    }
}
