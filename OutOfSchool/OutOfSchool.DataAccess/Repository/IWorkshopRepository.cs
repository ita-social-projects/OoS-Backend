using System;
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
    }
}