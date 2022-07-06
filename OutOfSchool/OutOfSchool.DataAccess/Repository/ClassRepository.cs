using System.Linq;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public class ClassRepository : EntityRepository<Class>, IClassRepository
{
    public ClassRepository(OutOfSchoolDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <inheritdoc/>
    public bool SameExists(Class entity) => dbSet.Any(x => (x.Title == entity.Title) && (x.DepartmentId == entity.DepartmentId));

    /// <inheritdoc/>
    public bool DepartmentExists(long id) => dbContext.Departments.Any(x => x.Id == id);
}