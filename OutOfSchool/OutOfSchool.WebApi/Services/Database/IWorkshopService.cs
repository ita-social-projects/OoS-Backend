using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services
{
    /// <summary>
    /// Defines interface for CRUD functionality for Workshop entity.
    /// </summary>
    public interface IWorkshopService : ICRUDService<WorkshopDTO, Guid>, IWorkshopImagesInteractionService
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

        Task<IEnumerable<Workshop>> GetByIds(IEnumerable<Guid> ids);

        /// <summary>
        /// Update prodider's properies in all workshops with specified provider.
        /// </summary>
        /// <param name="provider">Provider to be searched by.</param>
        /// <returns>List of Workshops for the specified provider.</returns>
        Task<IEnumerable<Workshop>> PartialUpdateByProvider(Provider provider);

        /// <summary>
        ///  Returns ProviderDto by Id of its own workshop entity.
        /// </summary>
        /// <param name="workshopId">WorkshopId for which we need to get provider owner entity.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<Guid> GetWorkshopProviderOwnerIdAsync(Guid workshopId);
    }
}
