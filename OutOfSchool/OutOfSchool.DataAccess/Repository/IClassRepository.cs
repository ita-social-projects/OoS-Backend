using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IClassRepository : IEntityRepository<Class>, IExistable<Class>
{
    /// <summary>
    /// Checks entity departmentId existens.
    /// </summary>
    /// <param name="id">Department id.</param>
    /// <returns>Bool.</returns>
    bool DepartmentExists(long id);
}