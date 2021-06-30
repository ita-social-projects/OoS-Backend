using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IDepartmentRepository : IEntityRepository<Department>
    {
        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        bool SameExists(Department entity);

        /// <summary>
        /// Checks entity directionId existens.
        /// </summary>
        /// <param name="id">Direction id.</param>
        /// <returns>Bool.</returns>
        bool DirectionExists(long id);
    }
}
