using System.Collections.Generic;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IChangesLogRepository : IEntityRepository<ChangesLog>
    {
        IEnumerable<ChangesLog> AddChangesLogToDbContext<TEntity>(TEntity entity, string userId)
            where TEntity : class, new();
    }
}
