using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class WorkshopRepository : EntityRepository<Workshop>, IWorkshopRepository
    {
        private readonly OutOfSchoolDbContext db;

        public WorkshopRepository(OutOfSchoolDbContext dbContext)
            : base(dbContext)
        {
            db = dbContext;
        }

        public IUnitOfWork UnitOfWork => db;

        /// <inheritdoc/>
        public new async Task Delete(Workshop entity)
        {
            if (entity.Applications?.Count > 0)
            {
                foreach (var app in entity.Applications)
                {
                    db.Entry(app).State = EntityState.Deleted;
                }
            }

            if (entity.Teachers?.Count > 0)
            {
                foreach (var teacher in entity.Teachers)
                {
                    db.Entry(teacher).State = EntityState.Deleted;
                }
            }

            db.Entry(entity).State = EntityState.Deleted;
            db.Entry(entity.Address).State = EntityState.Deleted;

            await db.SaveChangesAsync();
        }

        public async Task<Workshop> GetWithNavigations(long id)
        {
            return await db.Workshops
                .Include(ws => ws.Address)
                .Include(ws => ws.Teachers)
                .Include(ws => ws.DateTimeRanges)
                .ThenInclude(range => range.Workdays)
                .SingleOrDefaultAsync(ws => ws.Id == id);
        }

        /// <inheritdoc/>
        public bool ClassExists(long id) => db.Classes.Any(x => x.Id == id);
    }
}