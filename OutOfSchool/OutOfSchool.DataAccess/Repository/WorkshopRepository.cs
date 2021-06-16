using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        /// <inheritdoc/>
        public bool SubsubcategoryExists(long id) => db.Subsubcategories.Any(x => x.Id == id);
    }
}