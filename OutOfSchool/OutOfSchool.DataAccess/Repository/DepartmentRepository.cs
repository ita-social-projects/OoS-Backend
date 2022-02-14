using System.Linq;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public class DepartmentRepository : EntityRepository<Department>, IDepartmentRepository
    {
        private readonly OutOfSchoolDbContext db;

        public DepartmentRepository(OutOfSchoolDbContext dbContext)
         : base(dbContext)
        {
            db = dbContext;
        }

        /// <inheritdoc/>
        public bool SameExists(Department entity) => db.Departments.Any(x => (x.Title == entity.Title) && (x.DirectionId == entity.DirectionId));

        /// <inheritdoc/>
        public bool DirectionExists(long id) => db.Directions.Any(x => x.Id == id);
    }
}
