using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class SubcategoryRepository : EntityRepository<Subcategory>, ISubcategoryRepository
    {
        private readonly OutOfSchoolDbContext db;

        public SubcategoryRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        public bool SameExists(Subcategory entity) => db.Subcategories.Any(x => (x.Title == entity.Title) && (x.CategoryId == entity.CategoryId));

        /// <summary>
        /// Checks entity categoryId existens.
        /// </summary>
        /// <param name="id">Category id.</param>
        /// <returns>Bool.</returns>
        public bool CategoryExists(long id) => db.Categories.Any(x => x.Id == id);
    }
}
