using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services.ProviderAdminOperations;

/// <summary>
/// Defines interface for creating ProviderAdmin.
/// </summary>
public interface IProviderAdminOperationsService
{
    /// <summary>
    /// Create ProviderAdmin in IdentityServer.
    /// </summary>
    /// <param name="userId">User's identificator.</param>
    /// <param name="employeeDto">Entity for creation.</param>
    /// <param name="token">User's security token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<Either<ErrorResponse, CreateEmployeeDto>> CreateProviderAdminAsync(string userId, CreateEmployeeDto employeeDto, string token);
}