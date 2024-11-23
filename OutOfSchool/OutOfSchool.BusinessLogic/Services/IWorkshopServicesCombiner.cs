using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// The interface for CRUD operations with workshops.
/// </summary>
public interface IWorkshopServicesCombiner
{
    /// <summary>
    /// Add entity to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="WorkshopCreateUpdateDto"/>.</returns>
    Task<WorkshopDto> Create(WorkshopCreateRequestDto dto);

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
    /// Check if entity is exists by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    Task<bool> Exists(Guid id);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="asNoTracking">If true, the entity is retrieved without tracking.</param>
    /// <returns>A <see cref="Task{TEntity}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<WorkshopDto> GetById(Guid id, bool asNoTracking = false);

    /// <summary>
    /// Update existing entity in the database.
    /// </summary>
    /// <param name="dto">Entity that will be to updated.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="Result{Workshop}"/>
    /// that indicates the success or failure of the operation.
    /// If the operation succeeds, the <see cref="Result{Workshop}.Value"/> property
    /// contains the updated <see cref="WorkshopCreateUpdateDto"/>.
    /// If the operation fails, the <see cref="Result{Workshop}.OperationResult"/> property
    /// contains error information.</returns>
    Task<Result<WorkshopDto>> Update(WorkshopCreateUpdateDto dto);

    /// <summary>
    /// Update the Tags for existing Worskshop.
    /// </summary>
    /// <param name="dto">The Woskshop to be updated.</param>
    /// <returns>The updated <see cref="Workshop"/> entity if the update was successful, otherwise returns null.</returns>
    /// <remarks>
    /// This method will update the tags associated with a workshop based on the provided list of tag Ids.
    /// If the workshop does not exist, the method will return null.
    /// </remarks>
    Task<Result<WorkshopDto>> UpdateTags(WorkshopTagsUpdateDto dto);

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
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="List{WorkshopProviderViewCard}"/> that contains elements from the input sequence.</returns>
    Task<SearchResult<WorkshopProviderViewCard>> GetByProviderId(Guid id, ExcludeIdFilter filter);

    /// <summary>
    /// Get all entities that matches filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
    Task<SearchResult<WorkshopCard>> GetByFilter(WorkshopFilter filter);

    /// <summary>
    /// Get all entities that matches filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="IEnumerable{WorkshopES}"/> that contains elements that were found.</returns>
    Task<SearchResult<WorkshopCard>> GetByFilterForAdmins(WorkshopFilter filter);

    /// <summary>
    /// Update ProviderTitle property in all workshops with specified provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerTitle">Full Title of Provider to be changed.</param>
    /// <param name="providerTitleEn">Full English Title of Provider to be changed.</param>
    /// <returns><see cref="IEnumerable{T}"/> of Workshops for the specified provider.</returns>
    Task<IEnumerable<Workshop>> UpdateProviderTitle(Guid providerId, string providerTitle, string providerTitleEn);

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

    /// <summary>
    /// Update ProviderStatus property in all workshops with specified provider.
    /// </summary>
    /// <param name="providerId">Id of Provider to be searched by.</param>
    /// <param name="providerStatus">ProviderStatus of Provider to be changed.</param>
    /// <returns><see cref="IEnumerable{T}"/> of Workshops for the specified provider.</returns>
    Task<IEnumerable<ShortEntityDto>> UpdateProviderStatus(Guid providerId, ProviderStatus providerStatus);
}