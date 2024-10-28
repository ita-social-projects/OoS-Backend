using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

public interface IRegionAdminService
{
    /// <summary>
    /// Get entity by User id.
    /// </summary>
    /// <param name="id">Key of the User entity in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<RegionAdminDto> GetByUserId(string id);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<RegionAdminDto> GetByIdAsync(string id);

    /// <summary>
    /// Create Region Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="regionAdminBaseDto">Entity to add.</param>
    /// <param name="token">Valid token with RegionAdminAddNew permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, RegionAdminBaseDto>> CreateRegionAdminAsync(string userId, RegionAdminBaseDto regionAdminBaseDto, string token);

    /// <summary>
    /// Update Region Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="updateRegionAdminDto">Entity to update.</param>
    /// <param name="token">Valid token with RegionAdminUpdate permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, RegionAdminDto>> UpdateRegionAdminAsync(string userId, BaseUserDto updateRegionAdminDto, string token);

    /// <summary>
    /// Delete Region Admin.
    /// </summary>
    /// <param name="regionAdminId">Id of region admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with RegionAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> DeleteRegionAdminAsync(string regionAdminId, string userId, string token);

    /// <summary>
    /// Block Region Admin.
    /// </summary>
    /// <param name="regionAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with RegionAdminEdit permissions.</param>
    /// <param name="isBlocked">Block/unblock flag.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> BlockRegionAdminAsync(string regionAdminId, string userId, string token, bool isBlocked);

    /// <summary>
    /// Determines whether region admin is subordinate of the ministry admin.
    /// </summary>
    /// <param name="ministryAdminUserId">Ministry admin user id.</param>
    /// <param name="regionAdminId">RegionAdmin id.</param>
    /// <returns>The task result contains <see langword="true" /> if ergion admin is subordinate of the ministry admin
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsRegionAdminSubordinateAsync(string ministryAdminUserId, string regionAdminId);

    /// <summary>
    /// Get Region Admins from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{MinistryAdminDto}"/> that contains found elements.</returns>
    Task<SearchResult<RegionAdminDto>> GetByFilter(RegionAdminFilter filter);

    /// <summary>
    /// Reinvite Region Admin.
    /// </summary>
    /// <param name="regionAdminId">Id of region admin.</param>
    /// <param name="userId">Id of current user.</param>
    /// <param name="token">Valid token with RegionAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteRegionAdminAsync(
        string regionAdminId,
        string userId,
        string token);
}