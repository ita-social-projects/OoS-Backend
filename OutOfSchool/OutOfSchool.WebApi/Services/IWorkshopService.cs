using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService
    {
        /// <summary>
        /// Add entity.
        /// </summary>
        /// <param name="dto">Workshop to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        Task<IEnumerable<WorkshopDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>Workshop.</returns>
        Task<WorkshopDTO> GetById(long id);

        /// <summary>
        /// Get all workshops by organization Id.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Workshop entity to update.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(long id);

        /// <summary>
        /// Get count of pages of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="size">Count of records on one page.</param>
        /// <returns>COunt of pages.</returns>
        Task<int> GetPagesCount(WorkshopFilter filter, int size);

        /// <summary>
        /// Get page of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="size">Count of records on one page.</param>
        /// <param name="pageNumber">Number of page.</param>
        /// <returns>The list of workshops for this page.</returns>
        Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber);
    }
}