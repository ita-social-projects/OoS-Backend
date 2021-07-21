using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService : ICRUDService<WorkshopDTO>
    {
        /// <summary>
        /// Get all workshops by provider Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id);

        /// <summary>
        /// Get count of pages of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="size">Count of records on one page.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains <see cref="int"/> count of pages.</returns>
        Task<int> GetPagesCount(WorkshopFilter filter, int size);

        /// <summary>
        /// Get page of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="size">Count of records on one page.</param>
        /// <param name="pageNumber">Number of page.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation. The task result contains List of <see cref="WorkshopDTO"/> for this page.</returns>
        Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber);
    }
}