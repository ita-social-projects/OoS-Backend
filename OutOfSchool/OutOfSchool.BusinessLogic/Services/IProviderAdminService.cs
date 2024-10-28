using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IProviderAdminService
{
    Task<Either<ErrorResponse, CreateProviderAdminDto>> CreateProviderAdminAsync(
        string userId,
        CreateProviderAdminDto providerAdminDto,
        string token);

    Task<Either<ErrorResponse, UpdateProviderAdminDto>> UpdateProviderAdminAsync(
        UpdateProviderAdminDto providerAdminModel,
        string userId,
        Guid providerId,
        string token);

    Task<Either<ErrorResponse, ActionResult>> DeleteProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token);

    Task<Either<ErrorResponse, ActionResult>> BlockProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token,
        bool isBlocked);

    Task<Either<ErrorResponse, ActionResult>> BlockProviderAdminsAndDeputiesByProviderAsync(
        Guid providerId,
        string userId,
        string token,
        bool isBlocked);

    Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId);

    /// <summary>
    /// Get all provider admins from the database.
    /// </summary>
    /// <param name="userId">Current user's Id.</param>
    /// <param name="filter">Filter to get a part of all provider admins that were found.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The result is a <see cref="SearchResult{ProviderAdminDto}"/> that contains the count of all found provider admins and a list of provider admins that were received.</returns>
    /// <exception cref="ArgumentNullException">If one of the parameters was null.</exception>
    /// <exception cref="ArgumentException">If one of the offsetFilter's properties is negative.</exception>
    Task<SearchResult<ProviderAdminDto>> GetFilteredRelatedProviderAdmins(string userId, ProviderAdminSearchFilter filter);

    Task<IEnumerable<ProviderAdminDto>> GetRelatedProviderAdmins(string userId);

    Task<IEnumerable<Guid>> GetRelatedWorkshopIdsForProviderAdmins(string userId);

    Task<bool> CheckUserIsRelatedProviderAdmin(string userId, Guid providerId, Guid workshopId = default);

    Task<IEnumerable<string>> GetProviderAdminsIds(Guid workshopId);

    Task<IEnumerable<string>> GetProviderDeputiesIds(Guid providerId);

    /// <summary>
    /// Get workshops that provider admin can manage.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <param name="isProviderDeputy">Is providerAdmin deputy or no.</param>
    /// <returns>List of the workshops that providerAdmin can manage.</returns>
    Task<SearchResult<WorkshopProviderViewCard>> GetWorkshopsThatProviderAdminCanManage(string userId, bool isProviderDeputy);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="userId">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ProviderAdminProviderRelationDto> GetById(string userId);

    /// <summary>
    /// Get ProviderAdmin by it's Id.
    /// </summary>
    /// <param name="providerAdminId">ProviderAdmin's Id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<FullProviderAdminDto> GetFullProviderAdmin(string providerAdminId);

    /// <summary>
    /// Send invitation to ProviderAdmin by Id.
    /// </summary>
    /// <param name="providerAdminId">ProviderAdmin's Id.</param>
    /// <param name="userId">Current user's Id.</param>
    /// <param name="token">Current user's token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteProviderAdminAsync(
    string providerAdminId,
    string userId,
    string token);
}
