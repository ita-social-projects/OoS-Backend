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
        /// Create and add ChangesLog record for the Address entity converted to string. Log record is stored with ID of the parent entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
        /// <param name="entity">Entity with the modified address.</param>
        /// <param name="addressPropertyName">Address property name.</param>
        /// <param name="addressProjector">Function to project Address to string.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>An added ChangesLog record, or null.</returns>
        ChangesLog AddEntityAddressChangesLogToDbContext<TEntity>(TEntity entity, string addressPropertyName, Func<Address, string> addressProjector, string userId)
            where TEntity : class, IKeyedEntity, new();
    }
}
