using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services.ProviderAdminOperations;

/// <summary>
/// Defines interface for creating ProviderAdmin.
/// </summary>
public interface IProviderAdminOperationsService
{
    /// <summary>
    /// Create ProviderAdmin in IdentityServer.
    /// </summary>
    /// <param name="userId">User's identificator.</param>
    /// <param name="providerAdminDto">Entity for creation.</param>
    /// <param name="token">User's security token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<Either<ErrorResponse, CreateProviderAdminDto>> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdminDto, string token);
}