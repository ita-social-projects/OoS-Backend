using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IDepartmentRepository : IEntityRepository<Department>, IExistable<Department>
    {
        /// <summary>
        /// Checks entity directionId existens.
        /// </summary>
        /// <param name="id">Direction id.</param>
        /// <returns>Bool.</returns>
        bool DirectionExists(long id);
    }
}
