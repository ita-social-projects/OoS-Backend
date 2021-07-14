using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.ElasticsearchData.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// The interface for CRUD operations with workshops.
    /// </summary>
    public interface IWorkshopServicesProvider
    {
        /// <summary>
        /// Save the entity in data stores.
        /// </summary>
        /// <param name="dto">Workshop to add.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation. The task result contains a <see cref="WorkshopDTO"/> that was created.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="ClassDto"/> was not found.</exception>
        /// <exception cref="DbUpdateException">An exception that is thrown when an error is encountered while saving to the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        /// <exception cref="Exception">If some errors occure in Elasticsearch client.</exception>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Get the entity by it's key.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="WorkshopDTO"/> that was found, or null.</returns>
        Task<WorkshopDTO> GetById(long id);

        /// <summary>
        /// Update the entity.
        /// </summary>
        /// <param name="dto">Workshop entity to update.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="WorkshopDTO"/> that was updated.</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the <see cref="ClassDto"/> was not found.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        /// <exception cref="Exception">If some errors occure in Elasticsearch client.</exception>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        /// Delete the entity with the specified id.
        /// </summary>
        /// <param name="id">Workshop's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">If the workshop was not found in the database.</exception>
        /// <exception cref="DbUpdateConcurrencyException">If a concurrency violation is encountered while saving to database.</exception>
        /// <exception cref="Exception">If some errors occure in Elasticsearch client.</exception>
        Task Delete(long id);

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains found elements.</returns>
        Task<IEnumerable<WorkshopES>> GetAll();

        /// <summary>
        /// Get all workshops with the specified provider's Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopDTO}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopDTO>> GetByProviderId(long id);

        /// <summary>
        /// Get all entities that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
        Task<IEnumerable<WorkshopES>> GetByFilter(WorkshopFilterES filter);

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
