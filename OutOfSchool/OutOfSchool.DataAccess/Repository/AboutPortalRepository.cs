using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class AboutPortalRepository : SensitiveEntityRepository<AboutPortal>, IAboutPortalRepository
    {
        private readonly OutOfSchoolDbContext db;

        public AboutPortalRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        public IUnitOfWork UnitOfWork => db;

        public async Task<AboutPortal> GetWithNavigations()
        {
            return await db.AboutPortal
                .Include(ap => ap.AboutPortalItems)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public void DeleteAllItems()
        {
            db.AboutPortalItems.RemoveRange(db.AboutPortalItems);
        }

        public async Task CreateItems(IEnumerable<AboutPortalItem> entities)
        {
            await db.AboutPortalItems.AddRangeAsync(entities).ConfigureAwait(false);
        }
    }
}
