using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// The interface for CRUD operations with workshops.
    /// </summary>
    public interface IWorkshopServicesCombiner : ICRUDService<WorkshopDTO>
    {
        /// <summary>
        /// Get all entities from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a certain portion of all entities.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains the <see cref="IEnumerable{TEntity}"/> that contains found elements.</returns>
        Task<SearchResult<WorkshopCard>> GetAll(OffsetFilter offsetFilter);

        /// <summary>
        /// Get all workshop cards with the specified provider's Id.
        /// </summary>
        /// <param name="id">Provider's key.</param>
        /// <returns>A <see cref="Task{WorkshopCard}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="List{WorkshopCard}"/> that contains elements from the input sequence.</returns>
        Task<List<WorkshopCard>> GetByProviderId(long id);

        /// <summary>
        /// Get all entities that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter);
    }
}
