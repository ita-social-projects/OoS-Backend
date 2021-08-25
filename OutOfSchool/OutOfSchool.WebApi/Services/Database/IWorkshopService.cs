using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService : ICRUDService<WorkshopDTO, Guid>
    {
        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a certain portion of all entities.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopDTO>> GetAll(OffsetFilter offsetFilter);

        /// <summary>
        /// Get all workshops by provider Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopCard}"/> that contains elements from the input sequence.</returns>
        Task<IEnumerable<WorkshopCard>> GetByProviderId(Guid id);

        /// <summary>
        /// Get entities from the database that match filter's parameters.
        /// </summary>
        /// <param name="filter">Filter with specified searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="SearchResult{WorkshopCard}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter = null);

        /// <summary>
        /// Get entities from the database that match filter's parameters and sorts by distance.
        /// </summary>
        /// <param name="filter">Filter with specified searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="SearchResult{WorkshopCard}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopCard>> GetNearestByFilter(WorkshopFilter filter = null);
        Task<SearchResult<WorkshopDTO>> GetByFilter(WorkshopFilter filter = null);

        Task<IEnumerable<WorkshopDTO>> GetWorkshopsForCreate();

        Task<IEnumerable<WorkshopDTO>> GetWorkshopsForUpdate();

        Task<IEnumerable<long>> GetWorkshopsForDelete();
    }
}
