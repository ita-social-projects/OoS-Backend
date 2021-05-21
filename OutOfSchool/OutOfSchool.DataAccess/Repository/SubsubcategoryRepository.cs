using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class SubsubcategoryRepository : EntityRepository<Subsubcategory>, ISubsubcategoryRepository
    {
        private readonly OutOfSchoolDbContext db;

        public SubsubcategoryRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        public bool SameExists(Subsubcategory entity) => db.Subsubcategories.Any(x => (x.Title == entity.Title) && (x.SubcategoryId == entity.SubcategoryId));

        /// <summary>
        /// Checks entity subcategoryId existens.
        /// </summary>
        /// <param name="id">Subcategory id.</param>
        /// <returns>Bool.</returns>
        public bool SubcategoryExists(long id) => db.Subcategories.Any(x => x.Id == id);
    }
}
