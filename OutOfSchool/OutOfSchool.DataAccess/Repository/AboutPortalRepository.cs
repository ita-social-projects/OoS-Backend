using System;
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

        public async Task<AboutPortal> GetWithNavigations(Guid id)
        {
            return await db.AboutPortal
                .Include(ap => ap.AboutPortalItems)
                .SingleOrDefaultAsync(ap => ap.Id == id);
        }

        public async Task DeleteAllItems()
        {
            db.AboutPortalItems.RemoveRange(db.AboutPortalItems.Select(x => x));

            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
