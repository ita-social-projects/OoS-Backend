using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class WorkshopRepository : SensitiveEntityRepository<Workshop>, IWorkshopRepository
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

        public async Task<Workshop> GetWithNavigations(Guid id)
        {
            return await db.Workshops
                .Include(ws => ws.Address)
                .Include(ws => ws.Teachers)
                .Include(ws => ws.DateTimeRanges)
                .SingleOrDefaultAsync(ws => ws.Id == id);
        }

        /// <inheritdoc/>
        public bool ClassExists(long id) => db.Classes.Any(x => x.Id == id);

        public async Task<IEnumerable<Workshop>> GetWorkshopsForUpdate()
        {
            var elasticsearchSyncRecords = db.ElasticsearchSyncRecords;

            var resultMaxDate = from r in elasticsearchSyncRecords
                                group r by r.RecordId into gr
                                select new { RecordId = gr.Key, MaxOperationDate = gr.Max(r => r.OperationDate) };

            var resultCreate = from rMaxDate in resultMaxDate
                               join r in elasticsearchSyncRecords on new { rMaxDate.RecordId, OperationDate = rMaxDate.MaxOperationDate } equals new { r.RecordId, r.OperationDate } into leftJoin
                               from rg in leftJoin
                               where rg.Operation == Enums.ElasticsearchSyncOperation.Create
                               select new { rg.RecordId, rg.OperationDate, rg.Operation };

            return await db.Workshops.Join(resultCreate, u => u.Id, c => c.RecordId, (u, c) => u).ToListAsync();
        }
    }
}