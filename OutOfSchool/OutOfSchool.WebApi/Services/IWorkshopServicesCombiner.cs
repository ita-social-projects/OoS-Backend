using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services;

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
    /// Get all workshop cards (Id, Title) with the specified provider's Id.
    /// </summary>
    /// <param name="providerId">Provider's key.</param>
    /// <returns>A <see cref="Task{ShortEntityDto}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ShortEntityDto}"/> that contains elements from the input sequence.</returns>
    Task<List<ShortEntityDto>> GetWorkshopListByProviderId(Guid providerId);

    /// <summary>
    /// Get all workshop cards (Id, Title) with the specified provider admin's Id.
    /// </summary>
    /// <param name="providerAdminId">Provider admin's key.</param>
    /// <returns>A <see cref="Task{ShortEntityDto}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ShortEntityDto}"/> that contains elements from the input sequence.</returns>
    Task<List<ShortEntityDto>> GetWorkshopListByProviderAdminId(string providerAdminId);

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
    /// Update status field for existing entity in the database.
    /// </summary>
    /// <param name="dto">Workshop to update.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopStatusDto"/>.</returns>
    Task<WorkshopStatusDto> UpdateStatus(WorkshopStatusDto dto);

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
    /// <param name="filter">Filter to get a certain portion of all entities or exclude some entities by excluded ids.</param>
    /// <typeparam name="T">Type of entity that must be return.</typeparam>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{WorkshopBaseCard}"/> that contains elements from the input sequence.</returns>
    Task<SearchResult<T>> GetByProviderId<T>(Guid id, ExcludeIdFilter filter)
        where T : WorkshopBaseCard;

    /// <summary>
    /// Get all entities that matches filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
    Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter);

    /// <summary>
    /// Update ProviderTitle property in all workshops with specified provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerTitle">FullTitle of Provider to be changed.</param>
    /// <returns><see cref="IEnumerable{T}"/> of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle);

    /// <summary>
    /// Update IsBlocked property in all workshops with specified provider.
    /// </summary>
    /// <param name="provider">Provider to be searched by.</param>
    /// <returns><see cref="IEnumerable{T}"/> of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> BlockByProvider(Provider provider);

    /// <summary>
    /// Get id of provider, who owns Workshop with specefied id
    /// </summary>
    /// <param name="workshopId">WorkshopId to be searched by.</param>
    /// <returns>Guid id for the specified provider.</returns>
    Task<Guid> GetWorkshopProviderId(Guid workshopId);
}
