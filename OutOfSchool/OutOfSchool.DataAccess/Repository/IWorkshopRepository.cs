using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Services.Repository
{
    public interface IWorkshopRepository : IEntityRepository<Workshop>
    {
        IUnitOfWork UnitOfWork { get; }

        /// <summary>
        /// Checks entity classId existence.
        /// </summary>
        /// <param name="id">Class id.</param>
        /// <returns>True if Class exists, otherwise false.</returns>
        bool ClassExists(long id);

        Task<Workshop> UpdateWithNavigations(Workshop entity);

        Task<Workshop> GetWithNavigations(long id);
    }
}