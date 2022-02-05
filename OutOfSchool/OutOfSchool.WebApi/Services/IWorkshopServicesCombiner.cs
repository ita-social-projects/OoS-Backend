using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// The interface for CRUD operations with workshops.
    /// </summary>
    public interface IWorkshopServicesCombiner : ICRUDService<WorkshopDTO, Guid>, IWorkshopImagesInteractionService
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
        Task<List<WorkshopCard>> GetByProviderId(Guid id);

        /// <summary>
        /// Get all entities that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
        /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
        Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter);

        /// <summary>
        /// Update prodider's properies in all workshops with specified provider.
        /// </summary>
        /// <param name="provider">Provider to be searched by.</param>
        /// <returns>List of Workshops for the specified provider.</returns>
        Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider);


        /// <summary>
        /// Get id of provider, who owns Workshop with specefied id
        /// </summary>
        /// <param name="workshopId">WorkshopId to be searched by.</param>
        /// <returns>Guid id for the specified provider.</returns>
        Task<Guid> GetWorkshopProviderId(Guid workshopId);
    }
}
