using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OutOfSchool.Common.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services.Images;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Workshop entity.
/// </summary>
public interface IWorkshopService
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopDto"/>.</returns>
    Task<WorkshopBaseDto> Create(WorkshopBaseDto dto);

    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopResultDto"/>.</returns>
    Task<WorkshopResultDto> CreateV2(WorkshopV2Dto dto);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<WorkshopDto> GetById(Guid id);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopDto"/>.</returns>
    Task<WorkshopBaseDto> Update(WorkshopBaseDto dto);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopResultDto"/>.</returns>
    Task<WorkshopResultDto> UpdateV2(WorkshopV2Dto dto);

    /// <summary>
    /// Update status field for existing entity in the database.
    /// </summary>
    /// <param name="dto">Workshop id and status to update.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopStatusWithTitleDto"/>.</returns>
    Task<WorkshopStatusWithTitleDto> UpdateStatus(WorkshopStatusDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task DeleteV2(Guid id);

    /// <summary>
    /// Get all entities from the database.
    /// </summary>
    /// <param name="offsetFilter">Filter to get a certain portion of all entities.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{TEntity}"/> that contains found elements.</returns>
    Task<SearchResult<WorkshopDto>> GetAll(OffsetFilter offsetFilter);

    /// <summary>
    /// Get all workshops (Id, Title) by provider Id.
    /// </summary>
    /// <param name="providerId">Provider's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ShortEntityDto}"/> that contains elements from the input sequence.</returns>
    Task<List<ShortEntityDto>> GetWorkshopListByProviderId(Guid providerId);

    /// <summary>
    /// Get all workshops (Id, Title) by provider admin Id.
    /// </summary>
    /// <param name="providerAdminId">Provider admin's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{ShortEntityDto}"/> that contains elements from the input sequence.</returns>
    Task<List<ShortEntityDto>> GetWorkshopListByProviderAdminId(string providerAdminId);

    /// <summary>
    /// Get all workshops by provider Id.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <param name="filter">Filter to get a certain portion of all entities Or/And exclude by Workshop id.</param>
    /// <typeparam name="T">Type of entity that must be return.</typeparam>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SearchResult{WorkshopCard}"/> that contains elements from the input sequence.</returns>
    Task<SearchResult<T>> GetByProviderId<T>(Guid id, ExcludeIdFilter filter)
        where T : WorkshopBaseCard;

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
    /// Update ProviderTitle property in all workshops with specified provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerTitle">FullTitle of Provider to be changed.</param>
    /// <returns>List of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle);

    /// <summary>
    /// Update IsBloked property in all workshops with specified provider.
    /// </summary>
    /// <param name="provider">Provider to be searched by.</param>
    /// <returns>List of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> BlockByProvider(Provider provider);

    /// <summary>
    ///  Returns ProviderDto by Id of its own workshop entity.
    /// </summary>
    /// <param name="workshopId">WorkshopId for which we need to get provider owner entity.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Guid> GetWorkshopProviderOwnerIdAsync(Guid workshopId);

    /// <summary>
    ///  Returns isBlocked status of the workshop.
    /// </summary>
    /// <param name="workshopId">WorkshopId for which we need to get status.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<bool> IsBlocked(Guid workshopId);
}
