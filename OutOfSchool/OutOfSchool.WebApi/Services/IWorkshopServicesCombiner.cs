using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// The interface for CRUD operations with workshops.
    /// </summary>
    public interface IWorkshopServicesCombiner
    {
        /// <summary>
        /// Add entity to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopCreationResultDto"/>.</returns>
        Task<WorkshopDTO> Create(WorkshopDTO dto);

        /// <summary>
        /// Get entity by it's key.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
        /// The task result contains the entity that was found, or null.</returns>
        Task<WorkshopDTO> GetById(Guid id);

        /// <summary>
        /// Update existing entity in the database.
        /// </summary>
        /// <param name="dto">Entity that will be to updated.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopUpdateResultDto"/>.</returns>
        Task<WorkshopDTO> Update(WorkshopDTO dto);

        /// <summary>
        ///  Delete entity.
        /// </summary>
        /// <param name="id">Key in the table.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task Delete(Guid id);

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
        Task<List<WorkshopCard>> GetByProviderId(Guid id);

        /// <summary>
        /// Get all entities that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter);

        /// <summary>
        /// Update provider's properties in all workshops with specified provider.
        /// </summary>
        /// <param name="provider">Provider to be searched by.</param>
        /// <returns><see cref="IEnumerable{T}"/> of Workshops for the specified provider.</returns>
        Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider);
    }
}
