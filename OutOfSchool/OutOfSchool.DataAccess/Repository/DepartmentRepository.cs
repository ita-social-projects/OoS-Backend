using System.Linq;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class DepartmentRepository : EntityRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public bool SameExists(Department entity) => dbSet.Any(x => (x.Title == entity.Title) && (x.DirectionId == entity.DirectionId));

    /// <inheritdoc/>
    public bool DirectionExists(long id) => dbContext.Directions.Any(x => x.Id == id);
}