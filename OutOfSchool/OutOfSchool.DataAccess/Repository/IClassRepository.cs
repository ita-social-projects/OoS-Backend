using System;
using System.Collections.Generic;
using System.Text;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IClassRepository : IEntityRepository<Class>
    {
        /// <summary>
        /// Checks entity elements for uniqueness.
        /// </summary>
        /// <param name="entity">Entity.</param>
        /// <returns>Bool.</returns>
        bool SameExists(Class entity);

        /// <summary>
        /// Checks entity departmentId existens.
        /// </summary>
        /// <param name="id">Department id.</param>
        /// <returns>Bool.</returns>
        bool DepartmentExists(long id);
    }
}
