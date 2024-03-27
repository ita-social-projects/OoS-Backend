using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IParentRepository : IEntityRepositorySoftDeleted<Guid, Parent>
{
    /// <summary>
    /// Get Perents by theirs Ids.
    /// </summary>
    /// <param name="parentIds">Parent Ids.</param>
    /// <returns>List of Parents.</returns>
    public Task<IReadOnlyList<Parent>> GetByIdsAsync(IEnumerable<Guid> parentIds);
}