using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository;

public interface IChangesLogRepository : IEntityRepository<long, ChangesLog>
{
    /// <summary>
    /// Create and add ChangesLog records for the entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
    /// <param name="entity">Modified entity.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="trackedProperties">List of properties to be logged.</param>
    /// <param name="valueProjector">Function to project property value to string.</param>
    /// <returns>A collection of the added ChangesLog records.</returns>
    ICollection<ChangesLog> AddChangesLogToDbContext<TEntity>(
        TEntity entity,
        string userId,
        IEnumerable<string> trackedProperties,
        Func<Type, object, string> valueProjector)
        where TEntity : class, IKeyedEntity, new();
}
