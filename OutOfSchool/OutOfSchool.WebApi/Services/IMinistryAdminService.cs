using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services;

public interface IMinistryAdminService
{
    /// <summary>
    /// Create Ministry Admin
    /// </summary>
    /// <param name="userId">Id of user.</param>
    /// <param name="ministryAdminDto">Entity to add.</param>
    /// <param name="token">Valid token with MinistryAdminAddNew permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> CreateMinistryAdminAsync(string userId, CreateMinistryAdminDto ministryAdminDto, string token);

    /// <summary>
    /// Delete Ministry Admin
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token);

    /// <summary>
    /// Block Ministry Admin
    /// </summary>
    /// <param name="ministryAdminId">Id of ministry admin.</param>
    /// <param name="userId">Id of user.</param>
    /// <param name="token">Valid token with MinistryAdminEdit permissions.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<ResponseDto> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token);
}