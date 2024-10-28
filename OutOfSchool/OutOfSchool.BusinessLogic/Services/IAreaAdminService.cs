using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

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
    Task<Either<ErrorResponse, AreaAdminBaseDto>> CreateAreaAdminAsync(string userId, AreaAdminBaseDto areaAdminBaseDto, string token);

    /// <summary>
    /// Update Area Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="updateAreaAdminDto">Entity to update.</param>
    /// <param name="token">Valid token with AreaAdminUpdate permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, AreaAdminDto>> UpdateAreaAdminAsync(string userId, BaseUserDto updateAreaAdminDto, string token);

    /// <summary>
    /// Delete Area Admin.
    /// </summary>
    /// <param name="areaAdminId">Id of area admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with AreaAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> DeleteAreaAdminAsync(string areaAdminId, string userId, string token);

    /// <summary>
    /// Block Area Admin.
    /// </summary>
    /// <param name="areaAdminId">Id of area admin.</param>
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
    Task<bool> IsAreaAdminSubordinateMinistryAsync(string ministryAdminUserId, string areaAdminId);

    /// <summary>
    /// Determines whether area admin is subordinate of the region admin.
    /// </summary>
    /// <param name="regionAdminUserId">Region admin user id.</param>
    /// <param name="areaAdminId">AreaAdmin id.</param>
    /// <returns>The task result contains <see langword="true" /> if area admin is subordinate of the region admin
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsAreaAdminSubordinateRegionAsync(string regionAdminUserId, string areaAdminId);

    /// <summary>
    /// Determines whether area admin is subordinate of the ministry admin to be created.
    /// </summary>
    /// <param name="ministryAdminUserId">Ministry admin user id.</param>
    /// <param name="institutionId">Institution id.</param>
    /// <returns>The task result contains <see langword="true" /> if area admin is subordinate of the ministry admin to be created
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsAreaAdminSubordinateMinistryCreateAsync(string ministryAdminUserId, Guid institutionId);

    /// <summary>
    /// Determines whether area admin is subordinate of the region admin to be created.
    /// </summary>
    /// <param name="regionAdminUserId">Region admin user id.</param>
    /// <param name="institutionId">Institution id.</param>
    /// <param name="catottgId">CATOTTG id.</param>
    /// <returns>The task result contains <see langword="true" /> if area admin is subordinate of the region admin to be created
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsAreaAdminSubordinateRegionCreateAsync(string regionAdminUserId, Guid institutionId, long catottgId);

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
    /// <param name="areaAdminId">Id of area admin.</param>
    /// <param name="userId">Id of current user.</param>
    /// <param name="token">Valid token with RegionAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<Either<ErrorResponse, ActionResult>> ReinviteAreaAdminAsync(
        string areaAdminId,
        string userId,
        string token);
}