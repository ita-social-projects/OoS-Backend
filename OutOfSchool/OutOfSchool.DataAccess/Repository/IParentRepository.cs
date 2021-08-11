using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IParentRepository : IEntityRepository<Parent>
    {
        /// <summary>
        /// Find user onformation by parent Id.
        /// </summary>
        /// <param name="parents">Parent Ids.</param>
        /// <returns>Anonymous Type which contains part of user information.</returns>
        public IEnumerable<Tuple<long, string, string>> GetUsersByParents(IEnumerable<long> parents);
    }
}
