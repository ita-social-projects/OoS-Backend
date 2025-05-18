using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.Services.Repository.Api;

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

    /// <summary>
    /// Create and add ChangesLog records for the entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
    /// <param name="entity">Modified entity.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>An entity of the added ChangesLog record.</returns>
    Task<ChangesLog> AddCreatingOfEntityToChangesLog<TEntity>(
        TEntity entity,
        string userId)
        where TEntity : class, IKeyedEntity, new();

    /// <summary>
    /// Adds a collection of <see cref="ChangesLog"/> entries to the EF Core DbContext
    /// for tracking but does not persist them immediately to the database.
    /// </summary>
    /// <param name="logs">The collection of <see cref="ChangesLog"/> records to add.</param>
    /// <remarks>
    /// This method only registers the logs in the current unit of work.
    /// To persist them, <c>SaveChangesAsync()</c> must be called externally.
    /// </remarks>
    void AddChangeLogsToDbContext(IEnumerable<ChangesLog> logs);
}
