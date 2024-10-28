using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IMinistryAdminService
{
    /// <summary>
    /// Get entity by User id.
    /// </summary>
    /// <param name="id">Key of the User entity in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<MinistryAdminDto> GetByUserId(string id);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<MinistryAdminDto> GetByIdAsync(string id);

    /// <summary>
    /// Create Ministry Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="ministryAdminBaseDto">Entity to add.</param>
    /// <param name="token">Valid token with MinistryAdminAddNew permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, MinistryAdminBaseDto>> CreateMinistryAdminAsync(string userId, MinistryAdminBaseDto ministryAdminBaseDto, string token);

    /// <summary>
    /// Update Ministry Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="updateMinistryAdminDto">Entity to update.</param>
    /// <param name="token">Valid token with MinistryAdminUpdate permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, MinistryAdminDto>> UpdateMinistryAdminAsync(string userId, BaseUserDto updateMinistryAdminDto, string token);

    /// <summary>
    /// Delete Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token);

    /// <summary>
    /// Block Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <param name="isBlocked">Block/unblock flag.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token, bool isBlocked);

    /// <summary>
    /// Determines whether provider is subordinate of the ministry admin.
    /// </summary>
    /// <param name="ministryAdminUserId">Ministry admin user id.</param>
    /// <param name="providerId">Provider id.</param>
    /// <returns>The task result contains <see langword="true" /> if provider is subordinate of the ministry admin
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsProviderSubordinateAsync(string ministryAdminUserId, Guid providerId);

    /// <summary>
    /// Reinvite Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of current user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string token);
}