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

        /// <summary>
        /// Add new element.
        /// </summary>
        /// <param name="entity">Entity to create.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public new async Task<Workshop> Create(Workshop entity)
        {
            await db.Workshops.AddAsync(entity);
            await db.SaveChangesAsync();
            return await Task.FromResult(entity);
        }

        /// <summary>
        /// Delete element.
        /// </summary>
        /// <param name="entity">Entity to delete.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public new async Task Delete(Workshop entity)
        {
            if (entity.Teachers.Count > 0)
            {
                foreach (var teacher in entity.Teachers)
                {
                    db.Entry(teacher).State = EntityState.Deleted;
                }
            }
           
            db.Entry(entity).State = EntityState.Deleted;
            db.Entry(new Address { Id = entity.AddressId }).State = EntityState.Deleted;

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Checks entity subcategoryId existens.
        /// </summary>
        /// <param name="id">Subcategory id.</param>
        /// <returns>Bool.</returns>
        public bool SubsubcategoryExists(long id) => db.Subsubcategories.Any(x => x.Id == id);
    }
}