using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IParentRepository : IEntityRepository<Parent>
    {
        /// <summary>
        /// Find user information by parent Id.
        /// </summary>
        /// <param name="parents">Parent Ids.</param>
        /// <returns>Tuple which contains part of user information (parentId, firstName and lastName.</returns>
        public IEnumerable<(long parentId, string firstName, string lastName)> GetUsersByParents(IEnumerable<long> parents);
    }
}
