using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="WorkshopDTO"/> that was created.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetAll();

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="WorkshopDTO"/> that was found, or null.</returns>
        Task<WorkshopDTO> GetById(long id);

        /// <summary>
        /// Get all workshops by organization Id.
        /// </summary>
        /// <param name="id">Organization's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetWorkshopsByOrganization(long id);

        /// <summary>
        /// Update entity.
        /// </summary>
        /// <param name="dto">Workshop entity to update.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="WorkshopDTO"/> that was updated.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If entity was not found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        /// Delete entity.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        Task Delete(long id);

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
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains List of <see cref="WorkshopDTO"/> for this page.</returns>
        Task<List<WorkshopDTO>> GetPage(WorkshopFilter filter, int size, int pageNumber);
    }
}