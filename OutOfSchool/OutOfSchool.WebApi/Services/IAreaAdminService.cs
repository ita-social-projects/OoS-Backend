using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public interface IAreaAdminService
{
    /// <summary>
    /// Get entity by User id.
    /// </summary>
    /// <param name="id">Key of the User entity in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AreaAdminDto> GetByUserId(string id);

    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the entity that was found, or null.</returns>
    Task<AreaAdminDto> GetByIdAsync(string id);

    /// <summary>
    /// Create Area Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="areaAdminBaseDto">Entity to add.</param>
    /// <param name="token">Valid token with AreaAdminAddNew permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, AreaAdminBaseDto>> CreateAreaAdminAsync(string userId, AreaAdminBaseDto regionAdminBaseDto, string token);

    /// <summary>
    /// Update Area Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="updateAreaAdminDto">Entity to update.</param>
    /// <param name="token">Valid token with AreaAdminUpdate permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, AreaAdminDto>> UpdateAreaAdminAsync(string userId, AreaAdminDto updateRegionAdminDto, string token);

    /// <summary>
    /// Delete Area Admin.
    /// </summary>
    /// 
    /// <param name="areaAdminId">Id of area admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with AreaAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> DeleteAreaAdminAsync(string areaAdminId, string userId, string token);

    /// <summary>
    /// Block Area Admin.
    /// </summary>
    /// <param name="regionAreaId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with AreaAdminEdit permissions.</param>
    /// <param name="isBlocked">Block/unblock flag.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> BlockAreaAdminAsync(string areaAdminId, string userId, string token, bool isBlocked);

    /// <summary>
    /// Determines whether area admin is subordinate of the ministry admin.
    /// </summary>
    /// <param name="ministryAdminUserId">Ministry admin user id.</param>
    /// <param name="areaAdminId">AreaAdmin id.</param>
    /// <returns>The task result contains <see langword="true" /> if area admin is subordinate of the ministry admin
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsAreaAdminSubordinateAsync(string ministryAdminUserId, string areaAdminId);

    /// <summary>
    /// Get Area Admins from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{MinistryAdminDto}"/> that contains found elements.</returns>
    Task<SearchResult<AreaAdminDto>> GetByFilter(AreaAdminFilter filter);

    /// <summary>
    /// Reinvite Area Admin.
    /// </summary>
    /// <param name="areaAdminId">Id of region admin.</param>
    /// <param name="userId">Id of current user.</param>
    /// <param name="token">Valid token with RegionAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteAreaAdminAsync(
        string areaAdminId,
        string userId,
        string token);
}