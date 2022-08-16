using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

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
    Task<MinistryAdminDto> GetById(long id);

    /// <summary>
    /// Create Ministry Admin.
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="ministryAdminDto">Entity to add.</param>
    /// <param name="token">Valid token with MinistryAdminAddNew permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> CreateMinistryAdminAsync(string userId, CreateMinistryAdminDto ministryAdminDto, string token);

    /// <summary>
    /// Update Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminDto">Entity to update.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task<MinistryAdminDto> Update(MinistryAdminDto ministryAdminDto);

    /// <summary>
    /// Delete Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token);

    /// <summary>
    /// Block Ministry Admin.
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token);

    /// <summary>
    /// Determines whether provider is subordinate of the ministry admin.
    /// </summary>
    /// <param name="ministryAdminUserId">Ministry admin user id.</param>
    /// <param name="providerId">Provider id.</param>
    /// <returns>The task result contains <see langword="true" /> if provider is subordinate of the ministry admin
    /// filter; otherwise, <see langword="false" />.</returns>
    Task<bool> IsProviderSubordinateAsync(string ministryAdminUserId, Guid providerId);

    /// <summary>
    /// Get Ministry Admins from the database that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{MinistryAdminDto}"/> that contains found elements.</returns>
    Task<SearchResult<MinistryAdminDto>> GetByFilter(MinistryAdminFilter filter);
}