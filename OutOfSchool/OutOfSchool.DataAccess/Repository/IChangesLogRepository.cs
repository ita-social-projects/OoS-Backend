using System;
using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IChangesLogRepository : IEntityRepository<ChangesLog>
    {
        /// <summary>
        /// Create and add ChangesLog records for the entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
        /// <param name="entity">Modified entity.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="trackedFields">List of fields to be logged.</param>
        /// <returns>A collection of the added ChangesLog records.</returns>
        ICollection<ChangesLog> AddChangesLogToDbContext<TEntity>(TEntity entity, string userId, IEnumerable<string> trackedFields)
            where TEntity : class, IKeyedEntity, new();

        /// <summary>
        /// Create and add ChangesLog record for the property converted to string. Log record is stored with ID of the parent entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
        /// <typeparam name="TProperty">Property type.</typeparam>
        /// <param name="entity">Modified entity.</param>
        /// <param name="propertyName">Property name.</param>
        /// <param name="valueProjector">Function to project property value to string.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>An added ChangesLog record, or null.</returns>
        ChangesLog AddPropertyChangesLogToDbContext<TEntity, TProperty>(TEntity entity, string propertyName, Func<TProperty, string> valueProjector, string userId)
            where TEntity : class, IKeyedEntity, new();
    }
}
