using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services.ProviderServices;

/// <summary>
/// Defines interface for CRUD functionality for Provider entity.
/// </summary>
public interface IProviderService
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="providerDto">Provider entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderDto> Create(ProviderCreateDto providerDto);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Provider.</returns>
    Task<ProviderDto> GetById(Guid id);

    /// <summary>
    /// Get entity by User id.
    /// </summary>
    /// <param name="id">Key of the User entity in the table.</param>
    /// <param name="isEmployee">Is user a deputy or delegated provider admin.</param>
    /// <returns>Provider.</returns>
    Task<ProviderDto> GetByUserId(string id, bool isEmployee = false);

    /// <summary>
    /// Get provider's status.
    /// </summary>
    /// <param name="providerId">Key of the Provider entity in the table.</param>
    /// <returns>ProviderStatus.</returns>
    Task<ProviderStatusDto> GetProviderStatusById(Guid providerId);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="providerUpdateDto">Provider entity to add.</param>
    /// <param name="userId">Id of user that requests update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderDto> Update(ProviderUpdateDto providerUpdateDto, string userId);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <param name="token">Current user's token.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> Delete(Guid id, string token);

    /// <summary>
    ///  Gets Id of Provider, which owns a Workshop with specified Id.
    /// </summary>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<Guid> GetProviderIdForWorkshopById(Guid workshopId);

    /// <summary>
    /// Get blocked/unblocked status for provider.
    /// </summary>
    /// <param name="providerId">Key of the Provider entity in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.</returns>
    Task<bool?> IsBlocked(Guid providerId);

    /// <summary>
    /// Sends notification about provider
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="notificationAction"></param>
    /// <param name="addStatusData"></param>
    /// <param name="addLicenseStatusData"></param>
    /// <returns></returns>
    Task SendNotification(Provider provider, NotificationAction notificationAction, bool addStatusData, bool addLicenseStatusData);

    /// <summary>
    /// Updates workshop's provider status
    /// </summary>
    /// <param name="providerId"></param>
    /// <param name="providerStatus"></param>
    /// <returns></returns>
    Task UpdateWorkshopsProviderStatus(Guid providerId, ProviderStatus providerStatus);

    /// <summary>
    /// Check if entity is exists by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    Task<bool> Exists(Guid id);

    /// <summary>
    /// Checks whether the current user has Provider rights for the specified provider.
    /// </summary>
    /// <param name="providerId">The unique identifier of the provider.</param>    
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task HasProviderRights(Guid providerId);
}