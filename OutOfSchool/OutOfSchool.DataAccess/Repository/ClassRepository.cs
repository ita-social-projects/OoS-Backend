using System.Linq;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class ClassRepository : EntityRepository<Class>, IClassRepository
    {
        private readonly OutOfSchoolDbContext db;

        public ClassRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <inheritdoc/>
        public bool SameExists(Class entity) => db.Classes.Any(x => (x.Title == entity.Title) && (x.DepartmentId == entity.DepartmentId));

        /// <inheritdoc/>
        public bool DepartmentExists(long id) => db.Departments.Any(x => x.Id == id);
    }
}
