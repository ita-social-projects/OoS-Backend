using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepositoryBase<Guid, Workshop>
    {
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Checks entity classId existence.
        /// </summary>
        /// <param name="id">Class id.</param>
        /// <returns>True if Class exists, otherwise false.</returns>
        bool ClassExists(long id);

        Task<Workshop> GetWithNavigations(Guid id);

        Task<IEnumerable<Workshop>> GetListOfWorkshopsForSynchronizationByOperation(ElasticsearchSyncOperation operation);

        Task<IEnumerable<long>> GetListOfWorkshopIdsForSynchronizationByOperation(ElasticsearchSyncOperation operation);
    }
}
