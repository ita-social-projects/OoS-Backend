using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Update information about element.
        /// </summary>
        /// <param name="entity">Entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public new async Task<Workshop> Create(Workshop entity)
        {
            entity.SubcategoryId = db.Subsubcategories.Where(x => x.Id == entity.SubsubcategoryId).First().SubcategoryId;
            entity.CategoryId = db.Subcategories.Where(x => x.Id == entity.SubcategoryId).First().CategoryId;
            await db.Workshops.AddAsync(entity);
            await db.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Checks entity subcategoryId existens.
        /// </summary>
        /// <param name="id">Subcategory id.</param>
        /// <returns>Bool.</returns>
        public bool SubsubcategoryExists(long id) => db.Subsubcategories.Any(x => x.Id == id);
    }
}
