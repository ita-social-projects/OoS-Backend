using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for ChangesLog entity.
    /// </summary>
    public interface IChangesLogService
    {
        /// <summary>
        /// Create and add ChangesLog records for the entity.
        /// </summary>
        /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
        /// <param name="entity">Modified entity.</param>
        /// <param name="userId">User ID.</param>
        /// <returns>Number of the added ChangesLog records.</returns>
        int AddEntityChangesToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, new();

        Task<IEnumerable<ChangesLogDto>> GetChangesLog(ChangesLogFilter filter);
    }
}
