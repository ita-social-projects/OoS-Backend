using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Application entity.
/// </summary>
public interface IApplicationService
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="applicationDto">Application entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ModelWithAdditionalData<ApplicationDto, int>> Create(ApplicationCreate applicationDto);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Application.</returns>
    Task<ApplicationDto> GetById(Guid id);

    /// <summary>
    /// Get applications by workshop id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="providerId">Id of the workshop's provider.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAllByWorkshop(Guid id, Guid providerId, ApplicationFilter filter);

    /// <summary>
    /// Get applications by provider id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAllByProvider(Guid id, ApplicationFilter filter);

    /// <summary>
    /// Get applications by provider admin userId.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <param name="filter">Application filter.</param>
    /// <param name="providerId">Key in the table.</param>
    /// <param name="isDeputy">True if provider admin is deputy.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAllByEmployee(string userId, ApplicationFilter filter, Guid providerId = default, bool isDeputy = false);

    /// <summary>
    /// Get applications by parent id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="filter">Application filter.</param>
    /// <returns>List of applications.</returns>
    Task<SearchResult<ApplicationDto>> GetAllByParent(Guid id, ApplicationFilter filter);

    /// <summary>
    /// Get applications count by parent id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Count of applications.</returns>
    Task<int> GetCountByParentId(Guid id);

    /// <summary>
    /// Get applications by child id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>List of applications.</returns>
    Task<IEnumerable<ApplicationDto>> GetAllByChild(Guid id);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="applicationDto">Application entity to update.</param>
    /// <param name="providerId">Id of the provider for workshop.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ApplicationDto>> Update(ApplicationUpdate applicationDto, Guid providerId);

    /// <summary>
    /// Determines ability to create a new application for a child based on previous attempts.
    /// </summary>
    /// <param name="workshopId">Workshop id.</param>
    /// <param name="childId">Child id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains <see langword="true" /> if allowed to create a new application by the child status;
    /// otherwise, <see langword="false" />.</returns>
    Task<bool> AllowedNewApplicationByChildStatus(Guid workshopId, Guid childId);

    /// <summary>
    /// Check if exists an any application with approve status in workshop for parent.
    /// </summary>
    /// <param name="parentId">Parent's key.</param>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains <see langword="true" /> if exists an any application with approve status in workshop for parent;
    /// otherwise, <see langword="false" />.</returns>
    Task<bool> AllowedToReview(Guid parentId, Guid workshopId);

    /// <summary>
    /// Sets Studying status for all approved applications.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<int> ChangeApprovedStatusesToStudying();
}