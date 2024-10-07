using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services.EmployeeOperations;

/// <summary>
/// Defines interface for creating Employee.
/// </summary>
public interface IEmployeeOperationsService
{
    /// <summary>
    /// Create Employee in IdentityServer.
    /// </summary>
    /// <param name="userId">User's identificator.</param>
    /// <param name="employeeDto">Entity for creation.</param>
    /// <param name="token">User's security token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    public Task<Either<ErrorResponse, CreateEmployeeDto>> CreateEmployeeAsync(string userId, CreateEmployeeDto employeeDto, string token);
}